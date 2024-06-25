
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
            for (int i = CARDS.Count - 1; i >= 0; i--)
            {
                int j = random.Next(0, i + 1);
                PokerCard temp = CARDS[i];
                CARDS[i] = CARDS[j];
                CARDS[j] = temp;
            }

            remaining = new PokerCard[3] { CARDS[0], CARDS[1], CARDS[2] };

            //每人一张一张的切牌
            for (int i = 3; i < CARDS.Count; i++) { results[i % 3].Add(CARDS[i]); }

            return results;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 斗地主不洗牌算法
        /// </summary>
        /// <param name="remaining">剩余的3张牌</param>
        /// <param name="controlFactor">炸弹控制参数，此参数越小炸弹越多</param>
        /// <returns>3副初始牌，每副17张</returns>
        public static List<PokerCard>[] NoShuffle(out PokerCard[] remaining,int controlFactor=50)
        {
            //超过100已经接近完全随机了
            if(controlFactor >= 100) { return Shuffle(out remaining); }

            //每人最多20张牌
            List<PokerCard>[] results = new[] { new List<PokerCard>(20), new List<PokerCard>(20), new List<PokerCard>(20) };

            //先升序排列
            CARDS.Sort((p1, p2) => p1 - p2);

            Random random = new Random((int)DateTime.Now.Ticks);

            //有规则的随机打乱：炸弹随机交换
            int switchNum = random.Next(5, 20); //随机交换次数
            while(switchNum > 0)
            {
                int s1 = random.Next(3, 16); //牌码
                int s2 = random.Next(3, 16);
                //每4张为一组进行交换
                int c1 = (s1 - 3) * 4;
                int c2 = (s2 - 3) * 4;
                for (int j = 0; j < 4; j++)
                {
                    PokerCard poker = CARDS[c1+j];
                    CARDS[c1+j] = CARDS[c2+j];
                    CARDS[c2+j] = poker;
                }
                switchNum--;
            }
            //随机交换
            switchNum = random.Next(0, controlFactor < 0 ? 0 : controlFactor);

            while(switchNum > 0)
            {
                int r1 = random.Next(0, 54);
                int r2 = random.Next(0, 54);
                PokerCard poker = CARDS[r1];
                CARDS[r1] = CARDS[r2];
                CARDS[r2] = poker;
                switchNum--;
            }

            remaining = new PokerCard[3] { CARDS[0], CARDS[1], CARDS[2] };

            //一次性每人17张牌
            for (int i = 3; i < CARDS.Count; i++) {
                results[(i - 3) / 17].Add(CARDS[i]);
            }

            //将最后的牌再进行一次随机化交换
            int r = random.Next(0, 3);
            var result1 = results[r];
            results[r] = results[(r + 1) % 3];
            results[(r + 1) % 3] = result1;

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
                //对于单牌、顺子、连对、对子的情况比较元素差的和的大小
                if(currentType == PokerType.Single || currentType == PokerType.Double || currentType == PokerType.ShunZi || currentType== PokerType.LianDui)
                {
                    int sum = 0;
                    for (int i = 0; i < current.Count; i++)
                        sum += ((int)current[i] / 4 - (int)last[i] / 4);
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
        public bool FastCheck(List<PokerCard> current,List<PokerCard> last, PokerType lastType,out PokerType currentType)
        {
            currentType = PokerType.None;
            if(lastType == PokerType.None) { throw new NotSupportedException("上家出牌类型不能为PokerType.None类型!"); }

            if (IsKingBomb(current)) { currentType = PokerType.KingBomb; return true; }

            if(current.Count == last.Count)
            {
                switch (lastType)
                {
                    case PokerType.Single:
                        if (IsSingle(current)) { currentType = PokerType.Single; }
                        break;
                    case PokerType.Double:
                        if (IsDouble(current)) { currentType = PokerType.Double; }
                        break;
                    case PokerType.ShunZi:
                        if (IsShunZi(current)){ currentType = PokerType.ShunZi;}
                        break;
                    case PokerType.LianDui:
                        if(IsLiandDui(current)){ currentType = PokerType.LianDui; }
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

                if(currentType != PokerType.None)
                {
                    return Check(current, currentType, last, lastType);
                }
            }
            
            if(IsBomb(current))
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
                else if (IsLiandDui(cards)) { return PokerType.LianDui; }
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
        /// 该当前玩家出牌且上家没有人出牌时提示玩家出牌
        /// </summary>
        /// <remarks>注意：不会从玩家的手牌中移除提示的牌，这可能需要你自己完成这一步</remarks>
        /// <param name="cards">玩家的手牌</param>
        /// <param name="tipCards">存储提示要出的牌</param>
        /// <returns>出牌类型</returns>
        public static PokerType GetTipCards(List<PokerCard> cards, List<PokerCard> tipCards)
        {
            bool exitKingBomb = false;
            //(1单牌、2顺子、3对子、4连对、5三张、6飞机、7炸弹)
            var result = AnalysisCards(cards, ref exitKingBomb);
            //出单牌
            if (result.Item1 > 0)
            {
                ReadPokerCard(3, ref result.Item1, 1, tipCards);
                return PokerType.Single;
            }
            //出顺子
            else if (result.Item2 > 0)
            {
                var shunZi = GetContinusMinMax(3, ref result.Item2);
                ReadPokerCard(shunZi.Item1, shunZi.Item2, ref result.Item2, tipCards);
                return PokerType.ShunZi;
            }
            //出连对
            else if (result.Item4 > 0)
            {
                var lianDui = GetContinusMinMax(3, ref result.Item4);
                ReadPokerCard(lianDui.Item1, lianDui.Item2, ref result.Item4, tipCards);
                return PokerType.LianDui;
            }
            //出对子
            else if (result.Item3 > 0)
            {
                ReadPokerCard(3, ref result.Item3, 2, tipCards);
                return PokerType.Double;
            }
            //出飞机
            else if (result.Item6 > 0)
            {
                var feiJi = GetContinusMinMax(3, ref result.Item6);
                ReadPokerCard(feiJi.Item1, feiJi.Item2, ref result.Item6, tipCards);
                return PokerType.AeroplaneWithNone;
            }
            //出三张
            else if (result.Item5 > 0)
            {
                ReadPokerCard(3, ref result.Item5, 3, tipCards);
                return PokerType.ThreeWithNone;
            }
            //出炸弹
            else if (result.Item7 > 0)
            {
                ReadPokerCard(3, ref result.Item7, 4, tipCards);
                return PokerType.Bomb;
            }
            //出王炸
            else if (exitKingBomb)
            {
                tipCards.Add(PokerCard.Black_Joker);
                tipCards.Add(PokerCard.Red_Joker);
                return PokerType.KingBomb;
            }
            return PokerType.None;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 出牌提示：从当前玩家的手牌中获取到一副比上家出的牌要大的牌型
        /// </summary>
        /// <remarks>注意：不会从玩家的手牌中移除提示的牌，这可能需要你自己完成这一步</remarks>
        /// <param name="cards">当前玩家的手牌</param>
        /// <param name="outCards">上家出的牌</param>
        /// <param name="outCardsType">上家出的牌的类型</param>
        /// <param name="tipCards">提示玩家出的牌</param>
        /// <param name="tipType">提示出的牌的类型，如果没有则为None</param>
        /// <returns>当前提示出的牌，如果没有说明当前玩家的手牌不存在大于上家的牌</returns>
        public static void GetTipCards(List<PokerCard> cards, List<PokerCard> outCards, PokerType outCardsType, List<PokerCard> tipCards, out PokerType tipType)
        {
            //统计出玩家的牌的组成
            long outCardsCodes = 0;
            for (int i = 0; i < outCards.Count; i++)
                outCardsCodes |= 1L << (int)outCards[i];

            tipType = PokerType.None;
            //王炸可以直接跳过
            if (outCardsType != PokerType.KingBomb && outCardsType != PokerType.None)
            {
                //1. 分析玩家的手牌组成
                //(1单牌、2顺子、3对子、4连对、5三张、6飞机、7炸弹)
                bool existKingBomb = false;
                var result = AnalysisCards(cards, ref existKingBomb);

                //2. 根据上家的出牌类型进行分析当前应该出什么牌

                //2.1 如果当前玩家手牌少于上家出的牌，则只可能出炸弹了
                if (cards.Count < outCards.Count)
                {
                    //2.1.1 如果上家出的也是炸弹则只能出王炸
                    if (outCardsType == PokerType.Bomb && existKingBomb)
                    {
                        tipType = PokerType.KingBomb;
                        tipCards.Add(PokerCard.Black_Joker);
                        tipCards.Add(PokerCard.Red_Joker);
                        return;
                    }

                    //2.1.2 如果上家出的不是炸弹则只能出炸弹，取最小的炸弹
                    else if (result.Item7 != 0)
                    {
                        tipType = PokerType.Bomb;
                        int code = GetGreaterCode(ref result.Item7, 0);
                        tipCards.Add((PokerCard)((code - 3) * 4));
                        tipCards.Add((PokerCard)((code - 3) * 4 + 1));
                        tipCards.Add((PokerCard)((code - 3) * 4 + 2));
                        tipCards.Add((PokerCard)((code - 3) * 4 + 3));
                        return;
                    }
                }

                //2.2 如果当前玩家手牌数等于或多余上家出的牌数
                else if (cards.Count >= outCards.Count)
                {
                    //2.2.1 优先寻找与上家出的牌型一致的牌
                    switch (outCardsType)
                    {
                        case PokerType.Single:
                            {
                                int lastCode = (int)outCards[0] / 4 + 3;
                                if (GetSingleTip(ref result.Item1, ref lastCode, tipCards))
                                {
                                    tipType = PokerType.Single;
                                }
                            }
                            break;
                        case PokerType.Double:
                            {
                                int lastCode = (int)outCards[0] / 4 + 3;
                                if (GetDoubleTip(ref result.Item3, ref lastCode, tipCards))
                                {
                                    tipType = PokerType.Double;
                                }
                            }
                            break;
                        case PokerType.ShunZi:
                            {
                                if (result.Item2 > 0 && GetShunZiTip(ref result.Item2, ref outCardsCodes, tipCards))
                                {
                                    tipType = PokerType.ShunZi;
                                }
                            }
                            break;
                        case PokerType.LianDui:
                            {
                                if (result.Item4 > 0 && GetLianDuiTip(ref result.Item4, ref outCardsCodes, tipCards))
                                {
                                    tipType = PokerType.LianDui;
                                }
                            }
                            break;
                        case PokerType.ThreeWithNone:
                            {
                                int lastCode = GetWithCode(outCards);
                                //先从三张牌中进行提取
                                if (result.Item5 > 0 && GetThreeTip(ref result.Item5, ref lastCode, tipCards))
                                {
                                    tipType = PokerType.ThreeWithNone;
                                }
                                //再从飞机牌中进行提取
                                else if (result.Item6 > 0 && GetThreeTip(ref result.Item6, ref lastCode, tipCards))
                                {
                                    tipType = PokerType.ThreeWithNone;
                                }
                            }
                            break;
                        case PokerType.ThreeWithOne:
                            {
                                GetThreeWithOneTip(ref result, outCards, tipCards, ref tipType);
                            }
                            break;
                        case PokerType.ThreeWihtDouble:
                            {
                                if (GetThreeWithDoubleTip(ref result, outCards, tipCards))
                                {
                                    tipType = PokerType.ThreeWihtDouble;
                                }
                            }
                            break;
                        case PokerType.FourWithTwo:
                            {
                                if (GetFourWithTwoTip(ref result, outCards, tipCards))
                                {
                                    tipType = PokerType.FourWithTwo;
                                }
                            }
                            break;
                        case PokerType.FourWithDouble:
                            {
                                if (GetFourWithDoubleTip(ref result, outCards, tipCards))
                                {
                                    tipType = PokerType.FourWithDouble;
                                }
                            }
                            break;
                        case PokerType.FourWithTwoDouble:
                            {
                                if (GetFourWithTwoDoubleTip(ref result, outCards, tipCards))
                                {
                                    tipType = PokerType.FourWithTwoDouble;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithNone:
                            {
                                if (GetAeroplaneWithNoneTip(ref result, ref outCardsCodes, tipCards))
                                {
                                    tipType = PokerType.AeroplaneWithNone;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithTwo:
                            {
                                if (GetAeroplaneWithSingleTip(ref result, ref outCardsCodes, tipCards, 2))
                                {
                                    tipType = PokerType.AeroplaneWithTwo;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithThree:
                            {
                                if (GetAeroplaneWithSingleTip(ref result, ref outCardsCodes, tipCards, 3))
                                {
                                    tipType = PokerType.AeroplaneWithThree;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithFour:
                            {
                                if (GetAeroplaneWithSingleTip(ref result, ref outCardsCodes, tipCards, 4))
                                {
                                    tipType = PokerType.AeroplaneWithFour;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithFive:
                            {
                                if (GetAeroplaneWithSingleTip(ref result, ref outCardsCodes, tipCards, 5))
                                {
                                    tipType = PokerType.AeroplaneWithFive;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithTwoDouble:
                            {
                                if (GetAeroplaneWithDoubleTip(ref result, ref outCardsCodes, tipCards, 2))
                                {
                                    tipType = PokerType.AeroplaneWithTwoDouble;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithThreeDouble:
                            {
                                if (GetAeroplaneWithDoubleTip(ref result, ref outCardsCodes, tipCards, 3))
                                {
                                    tipType = PokerType.AeroplaneWithThreeDouble;
                                }
                            }
                            break;
                        case PokerType.AeroplaneWithFourDouble:
                            {
                                if (GetAeroplaneWithDoubleTip(ref result, ref outCardsCodes, tipCards, 4))
                                {
                                    tipType = PokerType.AeroplaneWithFourDouble;
                                }
                            }
                            break;
                        case PokerType.Bomb:
                            {
                                int lastCode = GetWithCode(outCards);
                                if (GetBombTip(ref result.Item7, ref lastCode, tipCards))
                                {
                                    tipType = PokerType.Bomb;
                                }
                            }
                            break;
                    }

                    //2.2.2 如果没有找到同类型的牌则出炸弹
                    if (tipType == PokerType.None)
                    {
			tipCards.Clear();//清空前面可能添加了的牌
			int lastCode = outCardsType == PokerType.Bomb ? GetWithCode(outCards) : 3;
                        if (result.Item7 > 0 && GetBombTip(ref result.Item7, ref lastCode, tipCards))
                        {
                            tipType = PokerType.Bomb;
                        }
                        else if (existKingBomb)
                        {
                            tipType = PokerType.KingBomb;
                            tipCards.Add(PokerCard.Black_Joker);
                            tipCards.Add(PokerCard.Red_Joker);
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------

        #region 提示算法的工具函数

        /// <summary>
        /// 从玩家指定的牌型中中找到一个大于指定牌码的牌码
        /// </summary>
        /// <returns>小于0等于0都表示不存在</returns>
        private static int GetGreaterCode(ref long codes, int compare)
        {
            int code = 3; //3、4、5、6...J、Q、K、A、2、小王、大王
            long temp = 15;
            while (code <= 17)
            {
                if ((codes & temp) > 0 && code > compare)
                {
                    return code;
                }
                temp <<= 4;
                code++;
            }
            return 0;
        }

        /// <summary>
        ///  从codes中从指定的牌码数开始读取指定几个PokerCard枚举值，从指定的startCode开始读取
        /// </summary>
        /// <param name="skipRead">指定是否为跳读模式，跳读模式则每次只会在一种类型的牌中读一张</param>
        private static void ReadPokerCard(int startCode, ref long codes, int readNum, List<PokerCard> cards, bool skipRead = false)
        {
            if (codes > 0 && readNum > 0)
            {
                long temp = 15L << (startCode - 3) * 4;
                long r = codes & temp;
                for (int i = startCode; i < 18; i++)
                {
                    if (r > 0)
                    {
                        int pokerCard = (i - 3) * 4;
                        for (int j = 0; j < 4 && readNum > 0; j++)
                        {
                            if (((r >> pokerCard) & 1) == 1)
                            {
                                readNum--;
                                cards.Add((PokerCard)pokerCard);
                                if (skipRead) { break; }
                            }
                            pokerCard++;
                        }
                    }
                    temp <<= 4;
                    r = codes & temp;
                }
            }
        }

        /// <summary>
        /// 从codes中读取指定牌码的PokerCard枚举值
        /// </summary>
        private static void ReadPokerCard(ref int code, ref long codes, List<PokerCard> cards)
        {
            //(code - 3) * 4 等于左移位数
            int pokerCard = (code - 3) * 4;
            long temp = 15L << pokerCard;
            long r = codes & temp;

            if (r > 0)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (((r >> pokerCard) & 1) == 1)
                    {
                        cards.Add((PokerCard)pokerCard);
                    }
                    pokerCard++;
                }
            }
        }

        /// <summary>
        /// 提取出指定范围的牌码中的所有的PokerCard [startCode,endCode]
        /// </summary>
        private static void ReadPokerCard(int startCode, int endCode, ref long codes, List<PokerCard> cards)
        {
            for (int code = startCode; code <= endCode; code++)
            {
                ReadPokerCard(ref code, ref codes, cards);
            }
        }


        /// <summary>
        /// 从指定的连续牌中获取最小和最大值
        /// </summary>
        private static ValueTuple<int, int> GetContinusMinMax(int startCode, ref long codes)
        {
            int min = 0, max = 0;
            if (codes > 0)
            {
                for (int code = startCode; code < 18; code++)
                {
                    //(code - 3) * 4 等于左移位数
                    int pokerCard = (code - 3) * 4;
                    long temp = 15L << pokerCard;
                    long r = codes & temp;

                    //找到第一个数
                    if (r > 0)
                    {
                        min = code;
                        while (r > 0)
                        {
                            code++;
                            pokerCard = (code - 3) * 4;
                            temp = 15L << pokerCard;
                            r = codes & temp;
                        }

                        max = code - 1;
                        break;
                    }
                }
            }

            return (min, max);
        }

        /// <summary>
        /// 分析玩家的手牌构成
        /// </summary>
        /// <returns>(单牌、顺子、对子、连对、三张、飞机、炸弹)</returns>
        private static ValueTuple<long, long, long, long, long, long, long> AnalysisCards(List<PokerCard> cards, ref bool existKingBomb)
        {
            long codes = 0;//玩家的手牌信息
            for (int i = 0; i < cards.Count; i++)
                codes |= 1L << (int)cards[i];

            //1. 分析出玩家手牌的构成

            // 76561193665298432 = 1L << 52 | 1L << 56;
            existKingBomb = (codes & 76561193665298432) == 76561193665298432;//玩家手中是否存在王炸
            long bombs = 0; //玩家手牌中的所有炸弹 --> 不含有王炸
            long shunZi = 0; //玩家手牌中的所有顺子
            long doubles = 0; //玩家手牌中的所有对子
            long singles = 0; //玩家手牌中的单牌
            long lianDui = 0; //玩家手牌中的连对
            long feiJi = 0; //玩家手牌中的飞机
            long threes = 0; //玩家手牌中的所有三张相同的牌

            long bomb = 15; //64位才能记下所有炸弹 //从最小的炸弹四个3开始 ==> 4个3转为位信息为 1111 => 15
            int code = 3; // 3、4、5、6...J、Q、K、A、2 、小王、大王 => 牌码

            long temp; int count = 0;
            while (code <= 17)
            {
                temp = (codes & bomb);
                CountOne(temp, ref count);

                switch (count)
                {
                    case 1:
                        singles |= temp;
                        if (code < 15) { shunZi |= temp; }
                        break;
                    case 2:
                        doubles |= temp;
                        if (code < 15) { lianDui |= temp; };
                        break;
                    case 3:
                        threes |= temp;
                        if (code < 15) { feiJi |= temp; }
                        break;
                    case 4: bombs |= temp; break;
                }

                bomb <<= 4; //左移4位得到下一个炸弹
                code++;
                count = 0;
            }

            //获得顺子牌
            ExcludeContinuous(ref shunZi, 5);
            //获取连对
            ExcludeContinuous(ref lianDui, 3);
            //获取飞机
            ExcludeContinuous(ref feiJi, 2);

            singles ^= shunZi;
            doubles ^= lianDui;
            threes ^= feiJi;

            if (existKingBomb) { singles ^= 76561193665298432; }

            return (singles, shunZi, doubles, lianDui, threes, feiJi, bombs);
        }

        /// <summary>
        /// 获取三带、四带中三或四的那张牌的牌码，如：3334，则返回3; 444422，则返回4
        /// </summary>
        private static int GetWithCode(List<PokerCard> cards)
        {
            int result = 0;
            int r = 0, n = 0, p, k, tmp, code, count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                code = 3 + ((int)cards[i]) / 4;
                tmp = 1 << code;
                p = (r & tmp) >> code; //判断第code位上是否为1
                k = (n & tmp) >> code;
                count += p + k;
                if (p == 1) { n |= tmp; }
                if (k == 1) { n &= ~tmp; }
                r |= tmp;
                if (count >= 3) { result = code; break; }
            }

            return result;
        }

        /// <summary>
        /// 统计temp中位为1的数量
        /// </summary>
        /// <param name="temp">要统计的值</param>
        /// <returns>统计出的数量</returns>
        private static void CountOne(long temp, ref int count)
        {
            //这个算法很简单只需要每次将这个temp值的最右边的1置为0即可
            while (temp > 0)
            {
                temp &= temp - 1;
                count++;
            }
        }

        /// <summary>
        /// 将exclude中连续的部分去掉
        /// </summary>
        /// <param name="exclude"></param>
        /// <param name="continuous">要连续几次才不进行去掉</param>
        private static void ExcludeContinuous(ref long exclude, int continuous)
        {
            int code = 3; long bomb = 15; int count = 0; long bomb2, temp, temp2;
            while (code <= 14)
            {
                temp = exclude & bomb;
                //找到右边不为0的那个
                if (temp != 0)
                {
                    count++;
                    bomb2 = bomb;

                    //从当前不为零的位置开始统计后面连续的次数
                    while (true)
                    {
                        bomb2 <<= 4; //左移4位得到下一个炸弹
                        temp = exclude & bomb2;
                        if (temp != 0) { count++; }
                        else { break; }
                    }

                    temp2 = bomb2;

                    //不是顺子，将shunZi的bomb到bomb2之间的位全部置为0
                    if (count < continuous)
                    {
                        temp = bomb2 | bomb;
                        while (bomb2 != bomb)
                        {
                            bomb2 >>= 4;
                            temp |= bomb2;
                        }
                        exclude &= ~temp;
                    }

                    bomb = temp2; //更新到最后那个截止位置
                    //更新code到最新值，减一的原因是最后还会加1
                    code += count - 1;
                }

                bomb <<= 4; //左移4位得到下一个炸弹
                code++;
                count = 0;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------

        #region 获取提示出牌类型

        /// <summary>
        /// 获取单牌的提示
        /// </summary>
        /// <param name="current">当前玩家的手牌的单牌的信息</param>
        /// <param name="lastCode">上家出牌的牌码</param>
        /// <param name="tipCards">记录提示的牌</param>
        /// <returns>是否提示成功</returns>
        private static bool GetSingleTip(ref long current, ref int lastCode, List<PokerCard> tipCards)
        {
            int code = GetGreaterCode(ref current, lastCode);
            if (code > 0)
                ReadPokerCard(ref code, ref current, tipCards);
            return tipCards.Count == 1;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 获取对子的提示
        /// </summary>
        /// <param name="current">当前玩家的手牌的对子牌的信息</param>
        /// <param name="lastCode">上家出牌的牌码</param>
        /// <param name="tipCards">记录提示的牌</param>
        /// <returns>是否提示成功</returns>
        private static bool GetDoubleTip(ref long current, ref int lastCode, List<PokerCard> tipCards)
        {
            int code = GetGreaterCode(ref current, lastCode);
            if (code > 0)
                ReadPokerCard(ref code, ref current, tipCards);
            return tipCards.Count == 2;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 获取顺子牌的提示
        /// </summary>
        /// <param name="current">当前玩家的手牌的顺子牌的信息</param>
        /// <param name="last">上家出牌的信息</param>
        /// <param name="tipCards">记录提示的牌</param>
        /// <returns>是否提示成功</returns>
        private static bool GetShunZiTip(ref long current, ref long last, List<PokerCard> tipCards)
        {
            int startCode = 3;
            var o = GetContinusMinMax(startCode, ref last); //出牌

            while (startCode <= 14)
            {
                var t = GetContinusMinMax(startCode, ref current); //提示

                if (t.Item1 == t.Item2) { break; }

                //两个顺子的长度差
                int deltaLen = (t.Item2 - t.Item1) - (o.Item2 - o.Item1);

                //只要当前长度大于上家时才可能
                if (deltaLen >= 0)
                {
                    //包含关系
                    //第一种包含关系：
                    // 3 4 5 6 7
                    // 3 4 5 6 7 8 9
                    //第二种：
                    //     5 6 7 8 9 10
                    // 3 4 5 6 7 8 9 10 11 
                    if (t.Item1 <= o.Item1 && t.Item2 > o.Item2)
                    {
                        ReadPokerCard(t.Item1 + (o.Item1 - t.Item1) + 1, o.Item2 + 1, ref current, tipCards);
                        break;
                    }

                    //交叉关系
                    // 5 6 7 8 9 
                    //   6 7 8 9 10 11
                    else if (t.Item1 > o.Item1 && t.Item2 > o.Item2)
                    {
                        ReadPokerCard(t.Item1, t.Item2 - deltaLen, ref current, tipCards);
                        break;
                    }
                }

                startCode = t.Item2 + 2;
            }

            return tipCards.Count >= 5;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 获取连对牌的提示
        /// </summary>
        /// <param name="current">当前玩家的手牌的连对牌的信息</param>
        /// <param name="last">上家出牌的信息</param>
        /// <param name="tipCards">记录提示的牌</param>
        /// <returns>是否提示成功</returns>
        private static bool GetLianDuiTip(ref long current, ref long last, List<PokerCard> tipCards)
        {
            int startCode = 3;
            var o = GetContinusMinMax(startCode, ref last); //出牌

            while (startCode <= 14)
            {
                var t = GetContinusMinMax(startCode, ref current); //提示

                if (t.Item1 == t.Item2) { break; }

                //两个连对的长度差
                int deltaLen = (t.Item2 - t.Item1) - (o.Item2 - o.Item1);

                //只要当前长度大于上家时才可能
                if (deltaLen >= 0)
                {
                    //包含关系
                    //第一种包含关系：
                    // 3 4 5 6 7
                    // 3 4 5 6 7 8 9
                    //第二种：
                    //     5 6 7 8 9 10
                    // 3 4 5 6 7 8 9 10 11 
                    if (t.Item1 <= o.Item1 && t.Item2 > o.Item2)
                    {
                        ReadPokerCard(t.Item1 + (o.Item1 - t.Item1) + 1, o.Item2 + 1, ref current, tipCards);
                        break;
                    }

                    //交叉关系
                    // 5 6 7 8 9 
                    //   6 7 8 9 10 11
                    else if (t.Item1 > o.Item1 && t.Item2 > o.Item2)
                    {
                        ReadPokerCard(t.Item1, t.Item2 - deltaLen, ref current, tipCards);
                        break;
                    }
                }

                startCode = t.Item2 + 2;
            }

            return tipCards.Count >= 6;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 获取三张牌的提示
        /// </summary>
        /// <param name="current">当前玩家的手牌的三张牌的信息</param>
        /// <param name="lastCode">上家的三张牌的牌码</param>
        /// <param name="tipCards">记录提示的牌</param>
        /// <returns>是否提示成功</returns>
        private static bool GetThreeTip(ref long current, ref int lastCode, List<PokerCard> tipCards)
        {
            int code = GetGreaterCode(ref current, lastCode);
            if (code > 0)
            {
                ReadPokerCard(ref code, ref current, tipCards);
                return true;
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetBombTip(ref long current, ref int lastCode, List<PokerCard> tipCards)
        {
            int code = GetGreaterCode(ref current, lastCode);
            if (code > 0)
            {
                ReadPokerCard(ref code, ref current, tipCards);
                return true;
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static void GetThreeWithOneTip(ref ValueTuple<long, long, long, long, long, long, long> result, List<PokerCard> outCards, List<PokerCard> tipCards, ref PokerType tipType)
        {
            //从单牌或对子或6顺中中抽一张最小的牌组成三带，如果抽不出来则不能组成牌型
            //优先从单牌中读取
            if (result.Item1 > 0) { ReadPokerCard(3, ref result.Item1, 1, tipCards); }
            //对子
            if (tipCards.Count == 0 && result.Item3 > 0) { ReadPokerCard(3, ref result.Item3, 1, tipCards); }
            //顺子
            if (tipCards.Count == 0 && result.Item2 > 0)
            {
                var minMax = GetContinusMinMax(3, ref result.Item2);
                while (minMax.Item1 != minMax.Item2)
                {
                    Console.WriteLine(minMax);
                    if (minMax.Item2 - minMax.Item1 > 4)
                    {
                        ReadPokerCard(minMax.Item1, ref result.Item2, 1, tipCards);
                        break;
                    }
                    else
                    {
                        minMax = GetContinusMinMax(minMax.Item2 + 1, ref result.Item2);
                    }
                }
            }
            //连对
            if (tipCards.Count == 0 && result.Item4 > 0) { ReadPokerCard(3, ref result.Item4, 1, tipCards); }

            if (tipCards.Count == 0) { return; }

            int lastCode = GetWithCode(outCards);

            //先从三张牌中进行提取
            if (result.Item5 > 0 && GetThreeTip(ref result.Item5, ref lastCode, tipCards))
            {
                tipType = PokerType.ThreeWithOne;
            }
            //再从飞机牌中进行提取
            else if (result.Item6 > 0 && GetThreeTip(ref result.Item6, ref lastCode, tipCards))
            {
                tipType = PokerType.ThreeWithOne;
            }

        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetThreeWithDoubleTip(ref ValueTuple<long, long, long, long, long, long, long> result, List<PokerCard> outCards, List<PokerCard> tipCards)
        {
            //(1单牌、2顺子、3对子、4连对、5三张、6飞机、7炸弹)
            if (result.Item3 > 0 || result.Item4 > 0)
            {
                int lastCode = GetWithCode(outCards);
                if (GetThreeTip(ref result.Item5, ref lastCode, tipCards))
                {
                    if (result.Item3 > 0)
                        ReadPokerCard(3, ref result.Item3, 2, tipCards);
                    else
                        ReadPokerCard(3, ref result.Item4, 2, tipCards);

                    return true;
                }
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetFourWithTwoTip(ref ValueTuple<long, long, long, long, long, long, long> result, List<PokerCard> outCards, List<PokerCard> tipCards)
        {
            if (result.Item1 > 0 && result.Item7 > 0)
            {
                ReadPokerCard(3, ref result.Item1, 2, tipCards);
                if (tipCards.Count == 2)
                {
                    int lastCode = GetWithCode(outCards);
                    if (GetBombTip(ref result.Item7, ref lastCode, tipCards))
                    {
                        return true;
                    }
                }
            }
            tipCards.Clear();
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetFourWithDoubleTip(ref ValueTuple<long, long, long, long, long, long, long> result, List<PokerCard> outCards, List<PokerCard> tipCards)
        {
            //(1单牌、2顺子、3对子、4连对、5三张、6飞机、7炸弹)
            if ((result.Item3 > 0 || result.Item4 > 0) && result.Item7 > 0)
            {
                int lastCode = GetWithCode(outCards);
                if (GetBombTip(ref result.Item7, ref lastCode, tipCards))
                {
                    if (result.Item3 > 0)
                        ReadPokerCard(3, ref result.Item3, 2, tipCards);
                    else
                        ReadPokerCard(3, ref result.Item4, 2, tipCards);

                    return true;
                }
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetFourWithTwoDoubleTip(ref ValueTuple<long, long, long, long, long, long, long> result, List<PokerCard> outCards, List<PokerCard> tipCards)
        {
            //(1单牌、2顺子、3对子、4连对、5三张、6飞机、7炸弹)
            if ((result.Item3 > 0 || result.Item4 > 0) && result.Item7 > 0)
            {
                int lastCode = GetWithCode(outCards);
                if (GetBombTip(ref result.Item7, ref lastCode, tipCards))
                {
                    if (result.Item3 > 0)
                        ReadPokerCard(3, ref result.Item3, 4, tipCards);
                    else
                        ReadPokerCard(3, ref result.Item4, 4, tipCards);

                    if (tipCards.Count == 8)
                    {
                        return true;
                    }
                }
            }

            tipCards.Clear();
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetAeroplaneWithNoneTip(ref ValueTuple<long, long, long, long, long, long, long> result, ref long outCardsCodes, List<PokerCard> tipCards)
        {
            int startCode = 3;
            var o = GetContinusMinMax(startCode, ref outCardsCodes); //出牌

            while (startCode <= 14)
            {
                var t = GetContinusMinMax(startCode, ref result.Item6); //提示

                if (t.Item1 == t.Item2) { break; }

                //长度差
                int deltaLen = (t.Item2 - t.Item1) - (o.Item2 - o.Item1);

                //只要当前长度大于或等于上家时才可能
                if (deltaLen >= 0)
                {
                    //包含关系
                    //第一种包含关系：
                    // 3 4 5 6 7
                    // 3 4 5 6 7 8 9
                    //第二种：
                    //     5 6 7 8 9 10
                    // 3 4 5 6 7 8 9 10 11 
                    if (t.Item1 <= o.Item1 && t.Item2 > o.Item2)
                    {
                        ReadPokerCard(t.Item1 + (o.Item1 - t.Item1) + 1, o.Item2 + 1, ref result.Item6, tipCards);
                        return true;
                    }

                    //交叉关系
                    // 5 6 7 8 9 
                    //   6 7 8 9 10 11
                    else if (t.Item1 > o.Item1 && t.Item2 > o.Item2)
                    {
                        ReadPokerCard(t.Item1, t.Item2 - deltaLen, ref result.Item6, tipCards);
                        return true;
                    }
                }

                startCode = t.Item2 + 2;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetAeroplaneWithSingleTip(ref ValueTuple<long, long, long, long, long, long, long> result, ref long outCardsCodes, List<PokerCard> tipCards, int withNum)
        {
            if (result.Item6 > 0)
            {
                //默认先从单牌中读取
                ReadPokerCard(3, ref result.Item1, withNum, tipCards);

                //从6顺中读取单牌
                //(1单牌、2顺子、3对子、4连对、5三张、6飞机、7炸弹)
                var shunZi = GetContinusMinMax(3, ref result.Item2);

                while (shunZi.Item2 != shunZi.Item1 && tipCards.Count < withNum)
                {
                    int len = shunZi.Item2 - shunZi.Item1 + 1;
                    if (len >= 6)
                    {
                        int remaing = withNum - tipCards.Count;
                        ReadPokerCard(shunZi.Item1, ref result.Item2, len - 5 > remaing ? remaing : len - 5, tipCards);
                    }
                    else
                    {
                        shunZi = GetContinusMinMax(shunZi.Item2 + 2, ref result.Item2);
                    }
                }

                //从对子中读取单牌
                ReadPokerCard(3, ref result.Item3, withNum - tipCards.Count, tipCards, true);

                //从连对中读取单牌
                ReadPokerCard(3, ref result.Item4, withNum - tipCards.Count, tipCards, true);

                //如果以上都没有凑足单牌数量，则说明不满足
                if (tipCards.Count == withNum && GetAeroplaneWithNoneTip(ref result, ref outCardsCodes, tipCards))
                {
                    return true;
                }

                tipCards.Clear();
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------

        private static bool GetAeroplaneWithDoubleTip(ref ValueTuple<long, long, long, long, long, long, long> result, ref long outCardsCodes, List<PokerCard> tipCards, int withDoubles)
        {
            withDoubles *= 2;
            if (result.Item6 > 0)
            {
                //从对子中读
                ReadPokerCard(3, ref result.Item3, withDoubles, tipCards);

                //从连对中读
                ReadPokerCard(3, ref result.Item4, withDoubles - tipCards.Count, tipCards);

                //如果以上都没有凑足单牌数量，则说明不满足
                if (tipCards.Count == withDoubles && GetAeroplaneWithNoneTip(ref result, ref outCardsCodes, tipCards))
                {
                    return true;
                }

                tipCards.Clear();
            }

            return false;
        }

        #endregion

        //------------------------------------------------------------------------------------------------------------

        #region 规则检查

        /// <summary>
        /// 单牌
        /// </summary>
        private static bool IsSingle(List<PokerCard> cards)
        {
            return cards.Count == 1;
        }

        /// <summary>
        /// 对子
        /// </summary>
        private static bool IsDouble(List<PokerCard> cards)
        {
            return cards.Count == 2 ? ((int)cards[0]) / 4 == ((int)cards[1]) / 4 : false;
        }

        /// <summary>
        /// 顺子
        /// </summary>
        private static bool IsShunZi(List<PokerCard> cards)
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
        private static bool IsLiandDui(List<PokerCard> cards)
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
        private static bool IsThreeWithNone(List<PokerCard> cards)
        {
            return cards.Count==3 && IsWith(cards,3);
        }

        /// <summary>
        /// 三带一
        /// </summary>
        private static bool IsThreeWithOne(List<PokerCard> cards)
        {
            return cards.Count == 4 && IsWith(cards, 3);
        }

        /// <summary>
        /// 三带一对
        /// </summary>
        private static bool IsThreeWihtDouble(List<PokerCard> cards)
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
        private static bool IsFourWithTwo(List<PokerCard> cards)
        {
            return cards.Count == 6 && IsWith(cards, 4);
        }

        /// <summary>
        /// 四带一对
        /// </summary>
        private static bool IsFourWithDouble(List<PokerCard> cards)
        {
            return cards.Count == 6 && IsWith(cards, 5);
        }

        /// <summary>
        /// 四带两对
        /// </summary>
        private static bool IsFourWithTwoDouble(List<PokerCard> cards)
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
        private static bool IsAeroplaneWithNone(List<PokerCard> cards)
        {
            return cards.Count % 3 == 0 && IsWith(cards, cards.Count);
        }

        /// <summary>
        /// 飞机带两张牌
        /// </summary>
        private static bool IsAeroplaneWithTwo(List<PokerCard> cards)
        {
            return cards.Count == 8 && IsAeroplaneWithWings(cards, 6);
        }

        /// <summary>
        /// 飞机带三张单牌
        /// </summary>
        private static bool IsAeroplaneWithThree(List<PokerCard> cards)
        {
            return cards.Count == 12 && IsAeroplaneWithWings(cards, 9);
        }

        /// <summary>
        /// 飞机带四张单牌
        /// </summary>
        private static bool IsAeroplaneWithFour(List<PokerCard> cards)
        {
            return cards.Count == 16 && IsAeroplaneWithWings(cards, 12);
        }

        /// <summary>
        /// 飞机带五张单牌
        /// </summary>
        private static bool IsAeroplaneWithFive(List<PokerCard> cards)
        {
            return cards.Count == 20 && IsAeroplaneWithWings(cards, 15);
        }

        /// <summary>
        /// 飞机带两对
        /// </summary>
        private static bool IsAeroplaneWithTwoDouble(List<PokerCard> cards)
        {
            return cards.Count == 10 && IsAeroplaneWithWings(cards, 8);
        }

        /// <summary>
        /// 飞机带三对
        /// </summary>
        private static bool IsAeroplaneWithThreeDouble(List<PokerCard> cards)
        {
            return cards.Count == 15 && IsAeroplaneWithWings(cards, 12);
        }

        /// <summary>
        /// 飞机带四对
        /// </summary>
        private static bool IsAeroplaneWithFourDouble(List<PokerCard> cards)
        {
            return cards.Count == 20 && IsAeroplaneWithWings(cards, 16);
        }

        /// <summary>
        /// 炸弹
        /// </summary>
        private static bool IsBomb(List<PokerCard> cards)
        {
            return cards.Count == 4 && IsWith(cards, 4);
        }

        /// <summary>
        /// 王炸
        /// </summary>
        private static bool IsKingBomb(List<PokerCard> cards)
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

        #endregion
    }
}
