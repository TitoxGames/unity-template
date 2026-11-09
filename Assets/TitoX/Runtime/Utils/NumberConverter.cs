using UnityEngine;

namespace TitoX.Utils
{
    public static class NumberConverter
    {
        public static string ReturnDamageString(double value)
        {
            string displayValue = System.Math.Round(value).ToString();
            int length = displayValue.Length;

            if (length < 7)
                return displayValue;

            return ReturnIdleMoneyString(value, 0, "", false);
        }

        public static string ReturnIdleMoneyString(int moneyValue, int decimalLength = 2, string cashString = "$", bool removeDecimalsIfZero = true)
        {
            string moneyString = moneyValue.ToString();
            return ReturnIdleMoneyString(moneyString, decimalLength, cashString, removeDecimalsIfZero);
        }

        public static string ReturnIdleMoneyString(double moneyValue, int decimalLength = 2, string cashString = "$", bool removeDecimalsIfZero = true)
        {
            moneyValue = System.Math.Floor(moneyValue);
            string moneyString = moneyValue.ToString("0." + new string('#', 339));

            string finalString = "";
            for (int i = 0; i < moneyString.Length; i++)
            {
                if (moneyString[i] == '.' || moneyString[i] == ',')
                {
                    break;
                }
                else
                {
                    finalString += moneyString[i];
                }
            }
            return ReturnIdleMoneyString(finalString, decimalLength, cashString, removeDecimalsIfZero);
        }

        private static string GetPostpend(int index)
        {
            switch (index)
            {
                case 1:
                    return "K";
                case 2:
                    return "M";
                case 3:
                    return "B";
                case 4:
                    return "T";
                case 5:
                    return "Q";
                case 6:
                    return "A";
                case 7:
                    return "B";
                case 8:
                    return "C";
                case 9:
                    return "D";
                case 10:
                    return "E";
                case 11:
                    return "F";
                case 12:
                    return "G";
                case 13:
                    return "H";
                case 14:
                    return "I";
                case 15:
                    return "J";
                case 16:
                    return "K";
                case 17:
                    return "L";
                case 18:
                    return "M";
                case 19:
                    return "N";
                case 20:
                    return "O";
                case 21:
                    return "P";
                case 22:
                    return "R";
                case 23:
                    return "S";
                case 24:
                    return "Z";
                case 25:
                    return "Y";
                default:
                    return "X";
            }
        }

        public static string ReturnIdleMoneyString(string moneyString, int decimalLength = 2, string cashString = "$", bool removeDecimalsIfZero = true)
        {
            if (moneyString.Length <= 3)
            {
                return cashString + moneyString;
            }

            float rawDiv = ((float)moneyString.Length - 1f) / 3f;
            float flooredDiv = Mathf.Floor(rawDiv);
            int numLeft = Mathf.RoundToInt((rawDiv - flooredDiv) * 3f) + 1;
            string finalString = cashString;
            for (int i = 0; i < numLeft; i++)
            {
                finalString += moneyString[i];
            }

            int numDecimalIndices = 0;

            for (int i = numLeft; i < Mathf.Clamp(numLeft + decimalLength, numLeft, moneyString.Length - 1); i++)
            {
                if (!removeDecimalsIfZero || moneyString[i] != '0')
                    numDecimalIndices++;
            }

            if (decimalLength > 0 && numDecimalIndices > 0)
                finalString += ".";

            for (int i = numLeft; i < Mathf.Clamp(numLeft + decimalLength, numLeft, moneyString.Length - 1); i++)
            {
                if (!removeDecimalsIfZero || moneyString[i] != '0')
                    finalString += moneyString[i];
            }

            return finalString + GetPostpend(Mathf.RoundToInt(flooredDiv));
        }
    }
}
