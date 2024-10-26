
using System.Text;

namespace PokerUtils
{
    public static class TestHelper
    {
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        public static List<PokerCard> GetRandomTypeCards(PokerType pokerType)
        {
            switch (pokerType)
            {
                case PokerType.None:
                    return None();
                case PokerType.Single:
                    return Single(random.Next(0, 18));
                case PokerType.Double:
                    return Double(random.Next(0, 16));
                case PokerType.ShunZi:
                    {
                        int len = random.Next(5, 13);
                        int startCode = random.Next(3, 12 - len + 4);
                        return ShunZi(startCode, startCode + len - 1);
                    }
                case PokerType.LianDui:
                    {
                        int len = random.Next(5, 13);
                        int startCode = random.Next(3, 12 - len + 4);
                        return LianDui(startCode, startCode + len - 1);
                    }
                case PokerType.ThreeWithNone:
                    {
                        int three = random.Next(3, 16);
                        return ThreeWith(three, 0);
                    }
                case PokerType.ThreeWithOne:
                    {
                        int three = random.Next(3, 16);
                        return ThreeWith(three, 1);
                    }
                case PokerType.ThreeWihtDouble:
                    {
                        int three = random.Next(3, 16);
                        return ThreeWith(three, 2);
                    }
                case PokerType.FourWithTwo:
                    return FourWithTwo(random.Next(3, 16));
                case PokerType.FourWithDouble:
                    return FourWithDouble(random.Next(3, 16), 1);
                case PokerType.FourWithTwoDouble:
                    return FourWithDouble(random.Next(3, 16), 2);
                case PokerType.AeroplaneWithNone:
                    {
                        int len = random.Next(5, 13);
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithNone(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithTwo:
                    {
                        int len = 2;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithSingle(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithThree:
                    {
                        int len = 3;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithSingle(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithFour:
                    {
                        int len = 4;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithSingle(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithFive:
                    {
                        int len = 5;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithSingle(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithTwoDouble:
                    {
                        int len = 2;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithDouble(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithThreeDouble:
                    {
                        int len = 3;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithDouble(startCode, startCode + len - 1);
                    }
                case PokerType.AeroplaneWithFourDouble:
                    {
                        int len = 4;
                        int startCode = random.Next(3, 12 - len + 4);
                        return AeroplaneWithDouble(startCode, startCode + len - 1);
                    }
                case PokerType.Bomb:
                    return Bomb(random.Next(3, 16));
                case PokerType.KingBomb:
                    return KingBomb();
            }

            throw new NotSupportedException("不在定义之中的类型");
        }

        public static string GetPokerString(List<PokerCard> cards, bool showOriginalStr = false)
        {
            if (showOriginalStr)
            {
                return $"[{string.Join(", ", cards)}]";
            }
            //♠ ♥ ♣ ♦
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < cards.Count; i++)
            {
                PokerCard card = cards[i];
                int code = 3 + (int)card / 4;
                int index = (int)card - (code - 3) * 4;
                if (code <= 15)
                {
                    switch (index)
                    {
                        case 0: builder.Append('♠'); break;
                        case 1: builder.Append('♥'); break;
                        case 2: builder.Append('♣'); break;
                        case 3: builder.Append('♦'); break;
                    }
                }
                switch (code)
                {
                    case 11: //J
                        builder.Append('J');
                        break;
                    case 12: //Q
                        builder.Append('Q');
                        break;
                    case 13: //K
                        builder.Append('K');
                        break;
                    case 14: //A
                        builder.Append('A');
                        break;
                    case 15: //2
                        builder.Append('2');
                        break;
                    case 16: // Joker
                        builder.Append("Joker");
                        break;
                    case 17: //King
                        builder.Append("King");
                        break;
                    default:
                        builder.Append(code);
                        break;
                }
                if (i != cards.Count - 1)
                {
                    builder.Append(',');
                    builder.Append(' ');
                }
            }
            builder.Append(']');
            return builder.ToString();
        }


        public static List<PokerCard> None()
        {
            List<PokerCard> cards = new(random.Next(0, 21));
            for (int i = 0; i < cards.Capacity; i++)
            {
                cards.Add((PokerCard)(random.Next(0, 18)));
            }
            return cards;
        }

        public static List<PokerCard> Single(int code)
        {
            List<PokerCard> cards = new(1);
            if (code <= 15)
                code = (code - 3) * 4 + random.Next(0, 4);
            else
                code = (code - 3) * 4;
            cards.Add((PokerCard)code);
            return cards;
        }

        public static List<PokerCard> Double(int code)
        {
            List<PokerCard> cards = new(2)
            {
                (PokerCard)((code - 3) * 4 + random.Next(0, 2)),
                (PokerCard)((code - 3) * 4 + random.Next(2, 4))
            };
            return cards;
        }

        public static List<PokerCard> ThreeWith(int code, int withNum)
        {
            List<PokerCard> cards = new(3 + withNum);
            int with = random.Next(0, 2) == 1 ? random.Next(3, code + 1) : random.Next(code + 1, 16);

            while (with == code)
            {
                with = random.Next(3, 16);
            }

            int ex = random.Next(0, 4);

            for (int i = 0; i < 4; i++)
            {
                if (i != ex)
                {
                    cards.Add((PokerCard)((code - 3) * 4 + i));
                }
            }

            if (withNum == 1)
            {
                cards.Add((PokerCard)((with - 3) * 4 + random.Next(0, 4)));
            }
            else if (withNum == 2)
            {
                cards.Add((PokerCard)((with - 3) * 4 + random.Next(0, 2)));
                cards.Add((PokerCard)((with - 3) * 4 + random.Next(2, 4)));
            }
            return cards;
        }

        public static List<PokerCard> ShunZi(int startCode, int endCode)
        {
            List<PokerCard> cards = new List<PokerCard>(endCode - startCode + 1);
            for (int i = startCode; i <= endCode; i++)
            {
                cards.Add((PokerCard)((i - 3) * 4 + random.Next(0, 4)));
            }
            return cards;
        }

        public static List<PokerCard> LianDui(int startCode, int endCode)
        {
            List<PokerCard> cards = new List<PokerCard>(endCode - startCode + 1);
            for (int i = startCode; i <= endCode; i++)
            {
                cards.Add((PokerCard)((i - 3) * 4 + random.Next(0, 2)));
                cards.Add((PokerCard)((i - 3) * 4 + random.Next(2, 4)));
            }
            return cards;
        }

        public static List<PokerCard> FourWithTwo(int code)
        {
            List<PokerCard> cards = new(6);

            while (cards.Count != 2)
            {
                int with1 = random.Next(3, 16);
                int with2 = random.Next(3, 16);
                if (with1 != code && with2 != code && with1 != with2)
                {
                    cards.Add((PokerCard)((with1 - 3) * 4 + random.Next(0, 4)));
                    cards.Add((PokerCard)((with2 - 3) * 4 + random.Next(0, 4)));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                cards.Add((PokerCard)((code - 3) * 4 + i));
            }

            return cards;
        }

        public static List<PokerCard> FourWithDouble(int code, int num)
        {
            List<PokerCard> cards = new(4 + 2 * num);

            if (num == 1)
            {
                while (cards.Count != 2)
                {
                    int with = random.Next(3, 16);
                    if (with != code)
                    {
                        cards.Add((PokerCard)((with - 3) * 4 + random.Next(0, 2)));
                        cards.Add((PokerCard)((with - 3) * 4 + random.Next(2, 4)));
                    }
                }
            }
            else if (num == 2)
            {
                while (cards.Count != 4)
                {
                    int with1 = random.Next(3, 16);
                    int with2 = random.Next(3, 16);
                    if (with1 != code && with2 != code && with1 != with2)
                    {
                        cards.Add((PokerCard)((with1 - 3) * 4 + random.Next(0, 2)));
                        cards.Add((PokerCard)((with1 - 3) * 4 + random.Next(2, 4)));
                        cards.Add((PokerCard)((with2 - 3) * 4 + random.Next(0, 2)));
                        cards.Add((PokerCard)((with2 - 3) * 4 + random.Next(2, 4)));
                    }
                }
            }


            for (int i = 0; i < 4; i++)
            {
                cards.Add((PokerCard)((code - 3) * 4 + i));
            }

            return cards;
        }

        public static List<PokerCard> AeroplaneWithNone(int startCode, int endCode)
        {
            List<PokerCard> cards = new List<PokerCard>((endCode - startCode + 1) * 3);
            for (int i = startCode; i <= endCode; i++)
            {
                int ex = random.Next(0, 4);
                for (int j = 0; j < 4; j++)
                {
                    if (j != ex)
                    {
                        cards.Add((PokerCard)((i - 3) * 4 + j));
                    }
                }
            }
            return cards;
        }

        public static List<PokerCard> AeroplaneWithSingle(int startCode, int endCode)
        {
            List<PokerCard> cards = AeroplaneWithNone(startCode, endCode);
            int withNum = endCode - startCode + 1;
            if (withNum <= 5 && withNum >= 2)
            {
                List<int> codes = new List<int>(withNum);
                while (codes.Count < withNum)
                {
                    int with = random.Next(0, 100) % 2 == 0 ? random.Next(3, startCode) : random.Next(endCode + 1, 16);
                    if (!codes.Contains(with)) { codes.Add(with); }
                }
                for (int i = 0; i < codes.Count; i++)
                {
                    cards.Add((PokerCard)((codes[i] - 3) * 4 + random.Next(0, 4)));
                }
            }
            else
            {
                throw new Exception("飞机最多只能带5张单牌，最少带两张");
            }
            return cards;
        }

        public static List<PokerCard> AeroplaneWithDouble(int startCode, int endCode)
        {
            List<PokerCard> cards = AeroplaneWithNone(startCode, endCode);
            int withNum = endCode - startCode + 1;
            if (withNum <= 4 && withNum >= 2)
            {
                List<int> codes = new List<int>(withNum);
                while (codes.Count < withNum)
                {
                    int with = random.Next(0, 100) % 2 == 0 ? random.Next(3, startCode) : random.Next(endCode + 1, 16);
                    if (!codes.Contains(with)) { codes.Add(with); }
                }
                for (int i = 0; i < codes.Count; i++)
                {
                    cards.Add((PokerCard)((codes[i] - 3) * 4 + random.Next(0, 2)));
                    cards.Add((PokerCard)((codes[i] - 3) * 4 + random.Next(2, 4)));
                }
            }
            else
            {
                throw new Exception("飞机最多只能带4个对子，最少带2个对子");
            }
            return cards;
        }

        public static List<PokerCard> KingBomb()
        {
            return new() { PokerCard.Black_Joker, PokerCard.Red_Joker };
        }

        public static List<PokerCard> Bomb(int code)
        {
            List<PokerCard> cards = new(4);

            for (int i = 0; i < 4; i++)
            {
                cards.Add((PokerCard)((code - 3) * 4 + i));
            }
            return cards;
        }
    }
}
