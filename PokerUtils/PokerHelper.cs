
namespace PokerUtils
{
    public static class PokerHelper
    {
        /// <summary>
        /// 54张扑克牌
        /// </summary>
        private static readonly List<PokerCard> CARDS;

        //-----------------------------------------------------------------------------------------------------------

        static PokerHelper()
        {
            CARDS = new List<PokerCard>(54) 
            {
                PokerCard.Spade_3,PokerCard.Heart_3,PokerCard.Club_3,PokerCard.Diamond_3,
                PokerCard.Spade_4,PokerCard.Heart_4,PokerCard.Club_4,PokerCard.Diamond_4,
                PokerCard.Spade_5,PokerCard.Heart_5,PokerCard.Club_5,PokerCard.Diamond_5,
                PokerCard.Spade_6,PokerCard.Heart_6,PokerCard.Club_6,PokerCard.Diamond_6,
                PokerCard.Spade_7,PokerCard.Heart_7,PokerCard.Club_7,PokerCard.Diamond_7,
                PokerCard.Spade_8,PokerCard.Heart_8,PokerCard.Club_8,PokerCard.Diamond_8,
                PokerCard.Spade_9,PokerCard.Heart_9,PokerCard.Club_9,PokerCard.Diamond_9,
                PokerCard.Spade_10,PokerCard.Heart_10,PokerCard.Club_10,PokerCard.Diamond_10,
                PokerCard.Spade_J,PokerCard.Heart_J,PokerCard.Club_J,PokerCard.Diamond_J,
                PokerCard.Spade_Q,PokerCard.Heart_Q,PokerCard.Club_Q,PokerCard.Diamond_Q,
                PokerCard.Spade_K,PokerCard.Heart_K,PokerCard.Club_K,PokerCard.Diamond_K,
                PokerCard.Spade_A,PokerCard.Heart_A,PokerCard.Club_A,PokerCard.Diamond_A,
                PokerCard.Spade_2,PokerCard.Heart_2,PokerCard.Club_2,PokerCard.Diamond_2,
                PokerCard.Black_Joker,PokerCard.Red_Joker
            };
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 斗地主随机洗牌
        /// </summary>
        /// <param name="remaining">剩余的3张牌</param>
        /// <returns>3副初始牌，每副17张</returns>
        public static List<PokerCard>[] Shuffle(out PokerCard[] remaining)
        {
            //每人最多20张牌
            List<PokerCard>[] results = new[]{ new List<PokerCard>(20), new List<PokerCard>(20), new List<PokerCard>(20) };

            Random random = new Random((int)DateTime.Now.Ticks);
            
            //洗牌算法，随着洗牌次数的增加将趋近完全随机化
            for (int i = CARDS.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                PokerCard temp = CARDS[i];
                CARDS[i] = CARDS[j];
                CARDS[j] = temp;
            }

            remaining = new PokerCard[3] { CARDS[0], CARDS[1], CARDS[2] };

            for (int i = 3; i < CARDS.Count; i++) { results[(i - 3) / 17].Add(CARDS[i]); }

            return results;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 判断当前玩家的出牌是否符合出牌规则
        /// </summary>
        /// <param name="current">当前玩家出的牌</param>
        /// <param name="currentType">当前玩家出的牌的类型</param>
        /// <param name="last">上一个玩家出的牌</param>
        /// <param name="lastType">上一个玩家出的牌的类型</param>
        /// <remarks>这种方式需要提前指定当前玩家出的牌的类型以及上家出的牌的类型</remarks>
        /// <returns>true:符合出牌规则；false:不符合出牌规则</returns>
        public static bool Check(List<PokerCard> current,PokerType currentType,List<PokerCard> last,PokerType lastType)
        {
            //不符合规则的牌型
            if(currentType == PokerType.None || lastType == PokerType.None) { return false; }

            //同一类型比较，牌数必须一致
            if(currentType == lastType && current.Count == last.Count)
            {
                //对于单牌、顺子、连对的情况比较元素差的和的大小
                if(currentType == PokerType.Single || currentType == PokerType.ShunZi || currentType== PokerType.LiandDui)
                {
                    int sum = 0;
                    for (int i = 0; i < current.Count; i++)
                        sum += (int)current[i] / 4 - (int)last[i] / 4;
                    return sum > 0;
                }

                //对于其它情况都可以通过与操作比较
                return AddOnlyDuplicateGreater2(current) - AddOnlyDuplicateGreater2(last) > 0;
            }

            //不是同一类型则必须是炸弹牌型
            return currentType - lastType > 0 && (int)currentType >= 19 ; 
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 判断当前玩家的出牌是否符合出牌规则，无需先获取当前出牌的类型。
        /// </summary>
        /// <param name="current">当前玩家出的牌</param>
        /// <param name="last">上一个玩家出的牌</param>
        /// <param name="lastType">上一个玩家出的牌的类型</param>
        /// <param name="currentType">当前玩家出牌类型</param>
        /// <remarks>此方法可以在无需提前知道当前玩家出的牌的类型</remarks>
        /// <returns>true:符合出牌规则；false:不符合出牌规则</returns>
        public static bool FastCheck(List<PokerCard> current,List<PokerCard> last, PokerType lastType,out PokerType currentType)
        {
            currentType = PokerType.None;
            if(lastType == PokerType.None) { throw new NotSupportedException("上家出牌类型不能为PokerType.None类型!"); }

            if (IsKingBomb(current)) { currentType = PokerType.KingBomb; return true; }

            if(current.Count == last.Count)
            {
                switch (lastType)
                {
                    case PokerType.Single:
                        currentType = PokerType.Single;
                        break;
                    case PokerType.Double:
                        if (IsDouble(current)) { currentType = PokerType.Double; }
                        break;
                    case PokerType.ShunZi:
                        if (IsShunZi(current)){ currentType = PokerType.ShunZi;}
                        break;
                    case PokerType.LiandDui:
                        if(IsLiandDui(current)){ currentType = PokerType.LiandDui; }
                        break;
                    case PokerType.ThreeWithNone:
                        if (IsThreeWithNone(current)){ currentType = PokerType.ThreeWithNone; }
                        break;
                    case PokerType.ThreeWithOne:
                        if (IsThreeWithOne(current)) { currentType = PokerType.ThreeWithOne; }
                        break;
                    case PokerType.ThreeWihtDouble:
                        if (IsThreeWihtDouble(current)) { currentType = PokerType.ThreeWihtDouble; }
                        break;
                    case PokerType.FourWithTwo:
                        if (IsFourWithTwo(current)) { currentType = PokerType.FourWithTwo; }
                        break;
                    case PokerType.FourWithDouble:
                        if (IsFourWithDouble(current)) { currentType = PokerType.FourWithDouble; }
                        break;
                    case PokerType.FourWithTwoDouble:
                        if (IsFourWithTwoDouble(current)) { currentType = PokerType.FourWithTwoDouble; }
                        break;
                    case PokerType.AeroplaneWithNone:
                        if (IsAeroplaneWithNone(current)) { currentType = PokerType.AeroplaneWithNone; }
                        break;
                    case PokerType.AeroplaneWithTwo:
                        if (IsAeroplaneWithTwo(current)) { currentType = PokerType.AeroplaneWithTwo; }
                        break;
                    case PokerType.AeroplaneWithThree:
                        if (IsAeroplaneWithThree(current)) { currentType = PokerType.AeroplaneWithThree; }
                        break;
                    case PokerType.AeroplaneWithFour:
                        if (IsAeroplaneWithFour(current)) { currentType = PokerType.AeroplaneWithFour; }
                        break;
                    case PokerType.AeroplaneWithFive:
                        if (IsAeroplaneWithFive(current)) { currentType = PokerType.AeroplaneWithFive; }
                        break;
                    case PokerType.AeroplaneWithTwoDouble:
                        if (IsAeroplaneWithTwoDouble(current)) { currentType = PokerType.AeroplaneWithTwoDouble; }
                        break;
                    case PokerType.AeroplaneWithThreeDouble:
                        if (IsAeroplaneWithThreeDouble(current)) { currentType = PokerType.AeroplaneWithThreeDouble; }
                        break;
                    case PokerType.AeroplaneWithFourDouble:
                        if (IsAeroplaneWithFourDouble(current)) { currentType = PokerType.AeroplaneWithFourDouble; }
                        break;
                    case PokerType.Bomb:
                        if (IsBomb(current)) { currentType = PokerType.Bomb; }
                        break;
                }

                return Check(current, currentType, last, lastType); 
            }
            else if(current.Count < last.Count && IsBomb(current))
            {
                currentType = PokerType.Bomb;
                return Check(current, currentType, last, lastType);
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 获取一副牌的类型
        /// </summary>
        /// <param name="cards">要判断的牌的类型</param>
        /// <returns>牌的类型</returns>
        public static PokerType GetPokerType(List<PokerCard> cards)
        {
            //按牌的奇数还是偶数进行减少条件判断
            //按牌的数量减少条件判断

            int count = cards.Count;
            bool isDouble = count % 2 == 0;
            if (isDouble && count < 5)
            {
                if (IsDouble(cards)) { return PokerType.Double; }
                else if (IsThreeWithOne(cards)) { return PokerType.ThreeWithOne; }
                else if (IsBomb(cards)) { return PokerType.Bomb; }
                else if (IsKingBomb(cards)) { return PokerType.KingBomb; }
            }
            else if(!isDouble && count <= 5)
            {
                if (IsSingle(cards)) { return PokerType.Single; }
                else if (IsThreeWithNone(cards)) { return PokerType.ThreeWithNone; }
                else if (IsThreeWihtDouble(cards)) { return PokerType.ThreeWihtDouble; }
            }

            if(count >= 5)
            {
                if (IsShunZi(cards)) { return PokerType.ShunZi; }
                else if (IsLiandDui(cards)) { return PokerType.LiandDui; }
                else if (IsFourWithTwo(cards)) { return PokerType.FourWithTwo; }
                else if (IsFourWithDouble(cards)) { return PokerType.FourWithDouble; }
                else if (IsFourWithTwoDouble(cards)) { return PokerType.FourWithTwoDouble; }
                else if (IsAeroplaneWithNone(cards)) { return PokerType.AeroplaneWithNone; }
                else if (IsAeroplaneWithTwo(cards)) { return PokerType.AeroplaneWithTwo; }
            }

            if (count >= 10)
            {
                if (IsAeroplaneWithThree(cards)) { return PokerType.AeroplaneWithThree; }
                else if (IsAeroplaneWithFour(cards)) { return PokerType.AeroplaneWithFour; }
                else if (IsAeroplaneWithFive(cards)) { return PokerType.AeroplaneWithFive; }
                else if (IsAeroplaneWithTwoDouble(cards)) { return PokerType.AeroplaneWithTwoDouble; }
                else if (IsAeroplaneWithThreeDouble(cards)) { return PokerType.AeroplaneWithThreeDouble; }
                else if (IsAeroplaneWithFourDouble(cards)) { return PokerType.AeroplaneWithFourDouble; }
            }

            return PokerType.None;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 单牌
        /// </summary>
        public static bool IsSingle(List<PokerCard> cards)
        {
            return cards.Count == 1;
        }

        /// <summary>
        /// 对子
        /// </summary>
        public static bool IsDouble(List<PokerCard> cards)
        {
            return cards.Count == 2 ? ((int)cards[0]) / 4 == ((int)cards[1]) / 4 : false;
        }

        /// <summary>
        /// 顺子
        /// </summary>
        public static bool IsShunZi(List<PokerCard> cards)
        {
            if(cards.Count >= 5)
            {
                int tmp = 0,temp, code, min=99;
                for (int i = 0; i < cards.Count; i++)
                {
                    code = 3 + ((int)cards[i]) / 4;
                    if(code < min) { min = code; }
                    temp = 1 << code;
                    if((tmp & temp) != 0 || code>14) { return false; } //顺子不能包含王和2
                    tmp |= temp;
                }
                
                return (tmp >>= (min + cards.Count)) == 0;
            }
            return false;
        }

        /// <summary>
        /// 连对
        /// </summary>
        public static bool IsLiandDui(List<PokerCard> cards)
        {
            int count = cards.Count;
            if (count >= 6 && count % 2 == 0)
            {
                int code, min = 99,max=0,sum=0;
                for (int i = 0; i < cards.Count; i++)
                {
                    code = 3 + ((int)cards[i]) / 4;
                    sum += code;
                    if (code > 14) { return false; } //不能包含王和2
                    if (code < min) { min = code; }
                    if(code > max) { max = code; }
                }

                return (min + max) * cards.Count /2 == sum;
            }
            return false;
        }

        /// <summary>
        /// 斗地主单出三张牌
        /// </summary>
        public static bool IsThreeWithNone(List<PokerCard> cards)
        {
            return cards.Count==3 && IsWith(cards,3);
        }

        /// <summary>
        /// 三带一
        /// </summary>
        public static bool IsThreeWithOne(List<PokerCard> cards)
        {
            return cards.Count == 4 && IsWith(cards, 3);
        }

        /// <summary>
        /// 三带一对
        /// </summary>
        public static bool IsThreeWihtDouble(List<PokerCard> cards)
        {
            bool f= cards.Count == 5 && IsWith(cards, 4);
            if (f)
            {
                int code, min = 99, max = 0, sum = 0;
                for (int i = 0; i < cards.Count; i++)
                {
                    code = 3 + ((int)cards[i]) / 4;
                    sum += code;
                    if (code < min) { min = code; }
                    if (code > max) { max = code; }
                }

                f = min * 3 + max * 2 == sum || min * 2 + max * 3 == sum;
            }
            return f;
        }

        /// <summary>
        /// 四带二
        /// </summary>
        public static bool IsFourWithTwo(List<PokerCard> cards)
        {
            return cards.Count == 6 && IsWith(cards, 4);
        }

        /// <summary>
        /// 四带一对
        /// </summary>
        public static bool IsFourWithDouble(List<PokerCard> cards)
        {
            return cards.Count == 6 && IsWith(cards, 5);
        }

        /// <summary>
        /// 四带两对
        /// </summary>
        public static bool IsFourWithTwoDouble(List<PokerCard> cards)
        {
            bool f = cards.Count == 8 && IsWith(cards, 6);
            if (f) //由于四带两对和飞机带两张的条件一致，需要再进一步杨筛选
            {
                int tmp = 0, code,count=0;
                for (int i = 0; i < cards.Count; i++)
                {
                    code = 3 + ((int)cards[i]) / 4;
                    tmp |= 1<<code;
                }
                while (tmp != 0)
                {
                    count += (tmp & 1);
                    tmp >>= 1;
                }
                f = count == 3;
            }
            return f;
        }

        /// <summary>
        /// 飞机什么都不带
        /// </summary>
        public static bool IsAeroplaneWithNone(List<PokerCard> cards)
        {
            return cards.Count % 3 == 0 && IsWith(cards, cards.Count);
        }

        /// <summary>
        /// 飞机带两张牌
        /// </summary>
        public static bool IsAeroplaneWithTwo(List<PokerCard> cards)
        {
            return cards.Count == 8 && IsAeroplaneWithWings(cards, 6);
        }

        /// <summary>
        /// 飞机带三张单牌
        /// </summary>
        public static bool IsAeroplaneWithThree(List<PokerCard> cards)
        {
            return cards.Count == 12 && IsAeroplaneWithWings(cards, 9);
        }

        /// <summary>
        /// 飞机带四张单牌
        /// </summary>
        public static bool IsAeroplaneWithFour(List<PokerCard> cards)
        {
            return cards.Count == 16 && IsAeroplaneWithWings(cards, 12);
        }

        /// <summary>
        /// 飞机带五张单牌
        /// </summary>
        public static bool IsAeroplaneWithFive(List<PokerCard> cards)
        {
            return cards.Count == 20 && IsAeroplaneWithWings(cards, 15);
        }

        /// <summary>
        /// 飞机带两对
        /// </summary>
        public static bool IsAeroplaneWithTwoDouble(List<PokerCard> cards)
        {
            return cards.Count == 10 && IsAeroplaneWithWings(cards, 8);
        }

        /// <summary>
        /// 飞机带三对
        /// </summary>
        public static bool IsAeroplaneWithThreeDouble(List<PokerCard> cards)
        {
            return cards.Count == 15 && IsAeroplaneWithWings(cards, 12);
        }

        /// <summary>
        /// 飞机带四对
        /// </summary>
        public static bool IsAeroplaneWithFourDouble(List<PokerCard> cards)
        {
            return cards.Count == 20 && IsAeroplaneWithWings(cards, 16);
        }

        /// <summary>
        /// 炸弹
        /// </summary>
        public static bool IsBomb(List<PokerCard> cards)
        {
            return cards.Count == 4 && IsWith(cards, 4);
        }

        /// <summary>
        /// 王炸
        /// </summary>
        public static bool IsKingBomb(List<PokerCard> cards)
        {
            return cards.Count == 2 && cards.Contains(PokerCard.Black_Joker) && cards.Contains(PokerCard.Red_Joker);
        }

        //-----------------------------------------------------------------------------------------
       
        /// <summary>
        /// <para>判断是否为三带、四带或什么都不带</para>
        /// 4个相同的牌可以统计出的次数为4; 
        /// 3个相同的牌可以统计出的次数为3; 
        /// 2个相同的牌可以统计出的次数为1;
        /// 其它为0
        /// </summary>
        private static bool IsWith(List<PokerCard> cards,int count)
        {
            int r = 0, n = 0, p, k, tmp, code;
            for (int i = 0; i < cards.Count; i++)
            {
                code = 3 + ((int)cards[i]) / 4;
                tmp = 1 << code;
                p = (r & tmp) >> code; //判断第code位上是否为1
                k = (n & tmp) >> code;
                count -= p + k;
                if (p == 1) { n |= tmp; }
                if (k == 1) { n &= ~tmp; }
                r |= tmp;
            }
            return count == 0;
        }

        /// <summary>
        /// 判断飞机带翅膀
        /// </summary>
        private static bool IsAeroplaneWithWings(List<PokerCard> cards,int count)
        {
            int r = 0, n = 0, p, k, tmp, code, min=100, max=0;
            for (int i = 0; i < cards.Count; i++)
            {
                code = 3 + ((int)cards[i]) / 4;
                tmp = 1 << code;
                p = (r & tmp) >> code; //判断第code位上是否为1
                k = (n & tmp) >> code;
                count -= p + k;
                if (p == 1) { n |= tmp; }
                if (k == 1) 
                { 
                    n &= ~tmp;
                    if(code > max) { max = code; }
                    if(code < min) { min = code; }
                }
                r |= tmp;
            }
            code = 0;
            for (int i = min; i <= max; i++)
                code += 1 << i; 
            return count == 0 && (r & code) == code; //判断是否连续
        }

        /// <summary>
        /// 对扑克牌中相同牌的数量大于2的求和
        /// </summary>
        private static int AddOnlyDuplicateGreater2(List<PokerCard> cards)
        {
            // sum只记录相同牌的数量>=3的类型，n只记录相同牌的数量>=2的类型
            int r = 0, n = 0, tmp, code, sum = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                code = 3 + ((int)cards[i]) / 4;
                tmp = 1 << code;
                if ((r & tmp) == tmp)
                {
                    if ((n & tmp) == tmp)
                    {
                        sum |= tmp;
                    }
                    n |= tmp;
                }
                r |= tmp;
              
            }
            return sum;
        }
    }
}
