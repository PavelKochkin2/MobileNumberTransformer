using System;
using System.Linq;
using System.Text.RegularExpressions;
using static System.String;

namespace HelloApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //number was "+375 25 123 123 123" => Country Code : "+375"; Mobile Number : "25 123123123"
            //number was "+(375) 25 123 123 123" => Country Code : "+375"; Mobile Number : "25 123123123"
            //number was "(+375) 25 123 123 123" => Country Code : "+375"; Mobile Number : "25 123123123"
            //number was " 25 123 123 123" => Country Code : "{organisation.CountryCode}"; Mobile Number : "123123123"
            //number was " 025 123 123 123" => Country Code : "{organisation.CountryCode}"; Mobile Number : "123123123"
            //number was "+(375)  025 123 123 123" => Country Code : "+375"; Mobile Number : "123123123"
            //number was "qweqweqwe" => Country Code : "{organisation.CountryCode}"; Mobile Number : null
            //number was "1qw1eq1we1qw1e" => Country Code : "{organisation.CountryCode}"; Mobile Number : null

            var result = new NumberTransformer().TransformNumber("+375 25 123 123 123");
            var result2 = new NumberTransformer().TransformNumber("+(375) 25 123 123 123");
            var resul3t = new NumberTransformer().TransformNumber("(+375) 25 123 123 123");
            var resul4t = new NumberTransformer().TransformNumber("25 123 123 123");
            var resul5t = new NumberTransformer().TransformNumber("025 123 123 123");
            var resul6t = new NumberTransformer().TransformNumber("+(375)  025 123 123 123");
            //var resul7t = new NumberTransformer().TransformNumber("qweqweqwe");
            //var resul8t = new NumberTransformer().TransformNumber("1qw1eq1we1qw1e");

            //0113 1112222
            //0113 111 2222
            //    + 113 111 2222
            //    (0113) 111 2222
            //    (0113) 1112222
            //0113 111 2222

            var resul24t = new NumberTransformer().TransformNumber("0113 1112222");
            var res123ult = new NumberTransformer().TransformNumber("0113 111 2222");
            var resu232lt = new NumberTransformer().TransformNumber("+ 113 111 2222");
            var resul232t = new NumberTransformer().TransformNumber("(0113) 111 2222");
            var res424ult = new NumberTransformer().TransformNumber("(0113) 1112222");
            var res2424ult = new NumberTransformer().TransformNumber("0113 111 2222");


        }
    }

    record PhoneNumber(string CountryCode, string Number);

    class NumberTransformer
    {
        private const int MAX_NUMBER_COUNTRYCODE_CHARS = 5;
        private static bool IsValid(string number) => new Regex("^[+0-9]+$").IsMatch(number);

        private static string CheckForLeadingZero(string number) => number.StartsWith("0") ? number.Replace("0", Empty) : number;

        public PhoneNumber TransformNumber(string number)
        {
            //write initial value to db to keep it

            string clearedNumber = number
                .Trim()
                .Replace("(", Empty)
                .Replace(")", Empty)
                .Replace("-", Empty)
                .Replace(" ", Empty);


            if (!IsValid(clearedNumber))
                throw new ArgumentException("Provided number is not valid", number);

            if (clearedNumber.StartsWith("00"))
                clearedNumber = clearedNumber.Replace("00", "+");
            

            var indexOrPlusChar = clearedNumber.IndexOf('+');

            bool numberContainsPlusChar = indexOrPlusChar != -1;

            //NUMBER CONTAINS COUNTRY CODE
            if (numberContainsPlusChar)
            {
                var possibleCodeStr = clearedNumber.Substring(indexOrPlusChar, indexOrPlusChar + MAX_NUMBER_COUNTRYCODE_CHARS);

                var countryCode = CheckCountryCodeForString(possibleCodeStr);

                if (!IsNullOrEmpty(countryCode))
                {
                    clearedNumber = clearedNumber.Replace(countryCode, Empty);
                }

                clearedNumber = CheckForLeadingZero(clearedNumber);

                //write new value(clearedNumber) to db

                return new PhoneNumber(countryCode, clearedNumber);
            }

            //NUMBER HAS NO COUNTRY CODE
            clearedNumber = CheckForLeadingZero(clearedNumber);

            //write to db new value

            return new PhoneNumber("organisation.CountryCode", clearedNumber);
        }

        private static string CheckCountryCodeForString(string possibleCodes)
        {
            if (possibleCodes.Length > MAX_NUMBER_COUNTRYCODE_CHARS)
                throw new InvalidOperationException();
            

            var possibleCountryCodes = new[]
            {
                possibleCodes[..2],
                possibleCodes[..3],
                possibleCodes[..4],
                possibleCodes[..5]
            };

            foreach (var country in CountryModel.Countries)
            {
                if (possibleCountryCodes.Any(country.Code.Equals))
                {
                    return country.Code;
                }
            }


            //log that no country code was found for the possible codes
            return Empty;

        }
    }
}