namespace PagarMePOS
{
    /// <summary>
    /// https://github.com/pagarme/docs.pagar.me/blob/master/docs/source/mpos.md#erros
    /// </summary>
    public enum PinPadErrors
    {
        CONNECTION_ERROR = -1,
        FLOW_OPERATION_ERROR = 10,
        PAGAR_ME_OPERATION_API_ERROR = 11,
        OPERATION_TIMEOUT = 12,
        OPERATION_CANCELED = 13,
        PINPAD_SESSION_INIT_ERROR = 15,
        EMV_TABLE_NOT_DOWLOADED_PROPERLY = 20,
        PINPAD_KEYS_NOT_LOADED = 42,
        CARD_REMOVED = 43,
        PINPAD_CARD_IS_NOT_WORKING_PROPERLY = 60,
        PINPAD_WRONG_CONFIG = 70,
        PAGAR_ME_ERROR = 90
    };

    public static class PinPadErrorsExtensions
    {
        public static string GetFriendlyMessage (this PinPadErrors error)
        {
            switch (error)
            {
                case PinPadErrors.CONNECTION_ERROR:
                    return "CONNECTION_ERROR";
                case PinPadErrors.FLOW_OPERATION_ERROR:
                    return "FLOW_OPERATION_ERROR";
                case PinPadErrors.PAGAR_ME_OPERATION_API_ERROR:
                    return "PAGAR_ME_OPERATION_API_ERROR";
                case PinPadErrors.OPERATION_TIMEOUT:
                    return "OPERATION_TIMEOUT";
                case PinPadErrors.OPERATION_CANCELED:
                    return "Operação cancelada";
                case PinPadErrors.PINPAD_SESSION_INIT_ERROR:
                    return "PINPAD_SESSION_INIT_ERROR";
                case PinPadErrors.EMV_TABLE_NOT_DOWLOADED_PROPERLY:
                    return "EMV_TABLE_NOT_DOWLOADED_PROPERLY";
                case PinPadErrors.PINPAD_KEYS_NOT_LOADED:
                    return "PINPAD_KEYS_NOT_LOADED";
                case PinPadErrors.PINPAD_CARD_IS_NOT_WORKING_PROPERLY:
                    return "PINPAD_CARD_IS_NOT_WORKING_PROPERLY";
                case PinPadErrors.PINPAD_WRONG_CONFIG:
                    return "PINPAD_WRONG_CONFIG";
                case PinPadErrors.CARD_REMOVED:
                    return "CARD_REMOVED";
                case PinPadErrors.PAGAR_ME_ERROR:
                    return "PAGAR_ME_ERROR";
                default:
                    return "UNKNOWN ERROR " + error.ToString();
            }
        }
    }

}
