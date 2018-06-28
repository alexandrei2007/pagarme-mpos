using PagarMe;
using PagarMe.Mpos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Threading.Tasks;

namespace PagarMePOS
{
    /// <summary>
    /// 
    /// </summary>
    public class PosService
    {
        #region Properties

        private string portName;
        private const int DISPLAY_MAXLENGTH = 32;
        private const int CARD_READ_TIMEOUT = 15;
        private string storagePath;
        private string[] cardBrands = { "visa", "mastercard", "amex", "elo", "diners" };

        #endregion

        #region Ctor

        public PosService()
        {
            this.portName = ConfigurationManager.AppSettings["PORT"];

            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["STORAGE_PATH"]))
                this.storagePath = AppDomain.CurrentDomain.BaseDirectory.ToString();
            else
                this.storagePath = ConfigurationManager.AppSettings["STORAGE_PATH"];

            PagarMeService.DefaultApiEndpoint = ConfigurationManager.AppSettings["PAGARME_API_ENDPOINT"];
            PagarMeService.DefaultEncryptionKey = ConfigurationManager.AppSettings["PAGARME_ENCRYPTION_KEY"];
            PagarMeService.DefaultApiKey = ConfigurationManager.AppSettings["PAGARME_API_KEY"];
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public async Task Display(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "";

            if (message.Length > DISPLAY_MAXLENGTH)
                message = message.Substring(0, DISPLAY_MAXLENGTH);

            using (var port = new SerialPort(this.portName))
            {
                port.Open();

                using (var mpos = CreateMposInstance(port))
                {
                    await mpos.Initialize();
                    mpos.Display(message.Trim());

                    await Task.Delay(2000);

                    await mpos.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="paymentMethod"></param>
        /// <returns></returns>
        public async Task CreateTransaction(int amount, PagarMe.Mpos.PaymentMethod paymentMethod)
        {
            Console.WriteLine(System.Environment.NewLine);

            using (var port = new SerialPort(this.portName))
            {
                port.Open();

                using (var mpos = CreateMposInstance(port))
                {
                    await mpos.Initialize();
                    
                    try
                    {
                        PaymentResult result = await mpos.ProcessPayment(amount, this.GetEmvApplications(paymentMethod), paymentMethod)
                            .TimeoutAfter(new TimeSpan(0, 0, CARD_READ_TIMEOUT));

                        if (result.Status == PaymentStatus.Accepted)
                        {
                            Transaction transaction = new Transaction();
                            transaction.Amount = amount;
                            transaction.CardHash = result.CardHash;

                            await transaction.SaveAsync();

                            int responseCode;
                            int.TryParse(transaction.AcquirerResponseCode, out responseCode);

                            object emvData = transaction["card_emv_response"];

                            await mpos.FinishTransaction(true, responseCode, (string)emvData);
                            await mpos.Close();

                            Console.WriteLine("TRANSACTION FINISHED: " + transaction.Id);
                            await Task.Delay(2000);
                        }
                        else
                        {
                            Console.WriteLine("TRANSACTION FAILED");
                            Console.WriteLine("ErrorCode:" + (result.ErrorCode != null ? ((PinPadErrors)result.ErrorCode).ToString() : "UNDEFINED"));
                            Console.WriteLine("Status:" + result.Status.ToString());
                        }
                    }
                    catch (PinPadException ex1)
                    {
                        Console.WriteLine("PinPadException: " + ex1.ErrorCode + " - " + ex1.Message);
                        mpos.Display(ex1.Message);
                        await Task.Delay(2000);
                    }
                    catch (TimeoutException ex2)
                    {
                        Console.WriteLine("TimeoutException: " + ex2.Message);
                        mpos.Display("CARD READ TIMEOUT");
                        await Task.Delay(2000);
                    }
                    catch (Exception ex3)
                    {
                        Console.WriteLine("Exception: " + ex3.Message);
                        mpos.Display("AN ERROR OCURRED");
                        await Task.Delay(2000);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task UpdateTables(bool force = true)
        {
            using (var port = new SerialPort(this.portName))
            {
                port.Open();

                using (var mpos = CreateMposInstance(port))
                {
                    await mpos.Initialize();
                    await mpos.SynchronizeTables(force);
                    await mpos.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        protected Mpos CreateMposInstance(SerialPort port)
        {
            var mpos = new Mpos(port.BaseStream, PagarMeService.DefaultEncryptionKey, this.storagePath);
            mpos.Initialized += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Initialized");
            };
            mpos.NotificationReceived += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NotificationReceived: {0}", (object)e);
            };
            mpos.TableUpdated += (sender, e) =>
            {
                mpos.Display("Tables Updated");
                System.Diagnostics.Debug.WriteLine("TableUpdated: {0}", e);
            };
            mpos.PaymentProcessed += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("PaymentProcessed: {0}, {1}, {2}", e.CardHash, e.PaymentMethod, e.Status);
            };
            mpos.FinishedTransaction += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("FinishedTransaction");
            };
            mpos.OperationCompleted += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("OperationCompleted");
            };
            mpos.Errored += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Errored: {0} {1}", e, ((PinPadErrors)e).GetFriendlyMessage());
            };

            return mpos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentMethod"></param>
        /// <returns></returns>
        protected List<EmvApplication> GetEmvApplications(PagarMe.Mpos.PaymentMethod paymentMethod)
        {
            var list = new List<EmvApplication>();

            foreach (var brand in this.cardBrands)
            {
                list.Add(new EmvApplication(brand, paymentMethod));
            }

            return list;
        }

        #endregion

    }
}
