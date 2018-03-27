using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum Status
    {
        [XmlEnum("SUCCESS")]
        Success,
        [XmlEnum("FAILED")]
        Failed,
        [XmlEnum("INPUT_ERROR")]
        InputError
    }

    public enum ReturnCode
    {
        [XmlEnum("AUTHORIZED")]
        Authorized,
        [XmlEnum("3DS_ENROLLED")]
        ThreeDSEnrolled,
        [XmlEnum("ALREADY_AUTHORIZED")]
        AlreadyAuthorized,
        [XmlEnum("AUTHORIZATION_FAILED")]
        AuthorizationFailed,
        [XmlEnum("INVALID_CUSTOMER_INFO")]
        InvalidCustomerInfo,
        [XmlEnum("INVALID_PAYMENT_INFO")]
        InvalidPaymentInfo,
        [XmlEnum("INVALID_ACCOUNT")]
        InvalidAccount,
        [XmlEnum("INVALID_PAYMENT_METHOD_CODE")]
        InvalidPaymentMethodCode,
        [XmlEnum("INVALID_CURRENCY")]
        InvalidCurrency,
        [XmlEnum("REQUEST_EXPIRED")]
        RequestExpired,
        [XmlEnum("HASH_MISMATCH")]
        HashMismatch,
        [Description("An error occurred during processing. Please retry the operation")]
        [XmlEnum("GW_ERROR_GENERIC")]
        Generic,
        [Description("An error occurred during 3DS processing")]
        [XmlEnum("GW_ERROR_GENERIC_3D")]
        ThreeDSProcessing,
        [Description("Error in card expiration date field")]
        [XmlEnum("GWERROR_-9")]
        InvalidExpirationDate,
        [XmlEnum("GWERROR_-3")]
        [Description("Call acquirer support call number")]
        CallAcquirer,
        [XmlEnum("GWERROR_-2")]
        [Description("An error occurred during processing. Please retry the operation")]
        UnknownError,
        [XmlEnum("GWERROR_05")]
        [Description("Authorization declined")]
        AuthorizationDeclined,
        [XmlEnum("GWERROR_08")]
        [Description("Invalid amount")]
        InvalidAmount1,
        [XmlEnum("GWERROR_13")]
        [Description("Invalid amount")]
        InvalidAmount2,
        [XmlEnum("GWERROR_14")]
        [Description("No such card")]
        NoSuchCard,
        [XmlEnum("GWERROR_15")]
        [Description("No such card/issuer")]
        NoSuchCardOrIssuer,
        [XmlEnum("GWERROR_19")]
        [Description("Re-enter transaction")]
        ReEnterTransction,
        [XmlEnum("GWERROR_34")]
        [Description("Credit card number failed the fraud")]
        Fraud,
        [XmlEnum("GWERROR_41")]
        [Description("Lost card")]
        Lost,
        [XmlEnum("GWERROR_43")]
        [Description("Stolen card, pick up")]
        Stolen,
        [XmlEnum("GWERROR_51")]
        [Description("Insufficient funds")]
        InsufficentFunds,
        [XmlEnum("GWERROR_54")]
        [Description("Expired card")]
        Expired,
        [XmlEnum("GWERROR_57")]
        [Description("Transaction not permitted on card")]
        NotPermitted,
        [XmlEnum("GWERROR_58")]
        [Description("Not permitted to merchant")]
        NotPermittedToMerchant,
        [XmlEnum("GWERROR_61")]
        [Description("Exceeds amount limit")]
        AmountLimitExceeded,
        [XmlEnum("GWERROR_62")]
        [Description("Restricted card")]
        Restricted,
        [XmlEnum("GWERROR_65")]
        [Description("Exceeds frequency limit")]
        FrequencyLimitExceeded,
        [XmlEnum("GWERROR_75")]
        [Description("PIN tries exceeded")]
        PinTriesExceeded,
        [XmlEnum("GWERROR_82")]
        [Description("Time-out at issuer")]
        TimeOutAtIssuer,
        [XmlEnum("GWERROR_84")]
        [Description("Invalid cvv")]
        InvalidCCV,
        [XmlEnum("GWERROR_91")]
        [Description("A technical problem occurred. Issuer cannot process")]
        TechnicalProblem,
        [XmlEnum("GWERROR_96")]
        [Description("System malfunction")]
        SystemMalfunction,
        [XmlEnum("GWERROR_2204")]
        [Description("No permission to process the card installment.")]
        NoPermissionForInstallment,
        [XmlEnum("GWERROR_2304")]
        [Description("There is an ongoing process your order.")]
        AlreadyInProcess,
        [XmlEnum("GWERROR_5007")]
        [Description("Debit cards only supports 3D operations.")]
        Only3D,
        [XmlEnum("NEW_ERROR")]
        [Description("Message flow error")]
        NewError,
        [XmlEnum("WRONG_ERROR")]
        [Description("Re-enter transaction")]
        WrongError,
        [XmlEnum("-9999")]
        [Description("Banned operation")]
        BannedOperation,
        [XmlEnum("1")]
        [Description("Call acquirer support call number")]
        CallAcquiererSupport
    }


    [DataContract]
    public class ThreeDSResponse
    {
        [DataMember(Name = "ALIAS")]
        public string Alias { get; set; }

        [DataMember(Name = "AMOUNT")]
        public decimal Amount { get; set; }

        [DataMember(Name = "CARD_PROGRAM_NAME")]
        public string CardProgramName { get; set; }

        [DataMember(Name = "CURRENCY")]
        public string Currency { get; set; }

        [DataMember(Name = "DATE")]
        public string Date { get; set; }

        [DataMember(Name = "HASH")]
        public string Hash { get; set; }

        [DataMember(Name = "INSTALLMENTS_NO")]
        public string InstallmentsNo { get; set; }

        [DataMember(Name = "REFNO")]
        public string ReferenceNo { get; set; }

        [DataMember(Name = "RETURN_CODE")]
        public ReturnCode ReturnCode { get; set; }

        [DataMember(Name = "RETURN_MESSAGE")]
        public string ReturnMessage { get; set; }

        [DataMember(Name = "STATUS")]
        public Status Status { get; set; }


        /// <summary>
        /// Gets 3ds response object from name value collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static ThreeDSResponse GetFromNameValueCollection(NameValueCollection collection)
        {
            Type type = typeof(ThreeDSResponse);
            var obj = Activator.CreateInstance(type);
            string value = "";

            foreach (var key in collection.AllKeys)
            {
                PropertyInfo property = type.GetProperties().Where(p => p
                                        .GetCustomAttributes(typeof(DataMemberAttribute), false)
                                        .OfType<DataMemberAttribute>()
                                        .Any(x => x.Name == key))
                                        .FirstOrDefault();

                value = collection.Get(key);

                if ((property.PropertyType == typeof(String) ||
                    property.PropertyType == typeof(Decimal) ||
                    property.PropertyType == typeof(DateTime) ||
                    property.PropertyType == typeof(Int32)) && value != string.Empty)
                {
                    property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                }
                else if (property.PropertyType.IsEnum)
                {

                    MemberInfo[] memberInfos = property.PropertyType.GetMembers(BindingFlags.Public | BindingFlags.Static);

                    MemberInfo enm = memberInfos.Where(m => m.GetCustomAttributes(typeof(XmlEnumAttribute), false).OfType<XmlEnumAttribute>().Any(x => x.Name == value))
                          .FirstOrDefault();

                    if (enm != null)
                    {
                        property.SetValue(obj, Enum.Parse(property.PropertyType, enm.Name));
                    }
                }

            }
            return (ThreeDSResponse)obj;

        }

    }
}
