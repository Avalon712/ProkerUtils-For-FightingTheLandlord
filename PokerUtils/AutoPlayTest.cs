
namespace PokerUtils
{
    //[MemoryDiagnoser]
    public sealed class AutoPlayTest
    {
        private static readonly List<PokerCard>[] results;
        private static readonly List<PokerCard> lastOutCards;
        private static readonly List<PokerCard> tipCards;
        static AutoPlayTest()
        {
            PokerCard[] remaining;
            results = PokerHelper.NoShuffle(out remaining, 20);
            //默认第一个玩家位地主
            results[0].AddRange(remaining);
            lastOutCards = new List<PokerCard>(10);
            tipCards = new List<PokerCard>(10);
        }

        //static void Main(string[] args)
        //{
        //    BenchmarkRunner.Run<Program>();
        //    Console.ReadLine();
        //}

        /*
        
        Benchmark测试两次的结果

        | Method   | Mean     | Error    | StdDev   | Allocated |
        |--------- |---------:|---------:|---------:|----------:|
        | TestPlay | 75.00 ns | 1.525 ns | 2.462 ns |         - |
        |--------- |---------:|---------:|---------:|----------:|
        | TestPlay | 72.35 ns | 1.444 ns | 1.483 ns |         - |

        */

        //[Benchmark]
        public void TestPlay()
        {
            int current = 0; //当前出牌玩家
            PokerType lastOutType = PokerType.None; //上家出牌的类型
            int last = 0;//上次玩牌的玩家
            // int round = 1; //当前回合数
            //int count = 3; //统计是否到了下一个回合

            while (true)
            {
                //if(count == 3) { Console.WriteLine($"第{round}回合"); round++; }

                // results[current].Sort((p1, p2) => p1 - p2); //方便查看

                //如果上次没有人要，则继续该当前玩家出牌
                if (last == current)
                {
                    lastOutCards.Clear();
                }

                //Console.WriteLine($"玩家{current}{(current == 0 ? "(地主)" : "农民")}回合阶段");

                if (lastOutCards.Count == 0)
                {
                    Console.WriteLine("出牌前的手牌: " + DebugHelper.GetPokerString(results[current]));

                    lastOutType = PokerHelper.GetTipCards(results[current], tipCards);
                    lastOutCards.AddRange(tipCards);
                    results[current].RemoveAll(p => tipCards.Contains(p));

                    //Console.WriteLine($"当前出牌为({lastOutType}): " + GetPokerString(tipCards));
                    //Console.WriteLine("出牌后的手牌: " + GetPokerString(results[current]));

                    tipCards.Clear();
                    last = current;
                }
                else
                {
                    //Console.WriteLine("出牌前的手牌: " + GetPokerString(results[current]));

                    PokerType currentType;
                    PokerHelper.GetTipCards(results[current], lastOutCards, lastOutType, tipCards, out currentType);

                    bool r = PokerHelper.FastCheck(tipCards, lastOutCards, lastOutType, out currentType);

                    if (currentType != PokerType.None)
                    {
                        lastOutCards.Clear();
                        lastOutCards.AddRange(tipCards);
                        results[current].RemoveAll(p => tipCards.Contains(p)); //从玩家的手牌中移除手牌
                        last = current;
                        lastOutType = currentType;
                    }

                    //Console.WriteLine($"当前出牌为({currentType})[{(r ? "√" : "×")}]: " + GetPokerString(tipCards));
                    //Console.WriteLine("出牌后的手牌: " + GetPokerString(results[current]));
                    tipCards.Clear();
                }

                //count--;
                //if(count == 0) { count = 3; Console.WriteLine(); }

                current = (current + 1) % 3; //下一家出牌
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Count == 0)
                    {
                        //Console.WriteLine($"对局结束，玩家{i}{(i == 0 ? "(地主)" : "农民")}胜利");
                        return;
                    }
                }
            }

        }

        public static string GetPokerString(List<PokerCard> cards)
        {
            return "[ " + string.Join(", ", cards) + " ]";
        }
    }


}
