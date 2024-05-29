# 一种基于位运算的斗地主超高性能算法

## 前言

​    最近在写棋牌游戏《快乐棋牌》时，关于其中斗地主玩法中检查玩家出牌是否符合规则、以及获取玩家当前出牌的类型（即：三带一、四带二、飞机、顺子等等）时，苦想一天。写了一个超高性能的算法，非常特别的一点是算法不仅执行速度非常快，而且不会使用任何数据结构、不开辟任何的堆内存、GC为零（算法采用位运算实现的，几个int值就搞定了，因此GC为零）！算法虽然采用的是C#写的，但是其中没有使用c#额外的任何API，可以方便转为任何其它编程语言。这儿就来讲一讲算法的核心思想。（这儿讲的零GC是指非洗牌算法，因为洗牌要获得每个玩家的手牌，无法避免要new对象）。

​    下面表格是测试提供的两种算法的性能测试，可以看见没有开辟任何堆内存，GC分配为零。

| Method(测试方法) | Mean (平均执行耗时) | Error (偏差) | StdDev (标准差) | Allocated(内存分配) |
| :--------------: | ------------------- | ------------ | --------------- | ------------------- |
|    Test_Check    | 168.199 ns          | 0.9374 ns    | 0.7828 ns       | -                   |
|  Test_FastCheck  | 8.219 ns            | 0.0539 ns    | 0.0478 ns       | -                   |

## 算法定义

​    使用PokerCard这个枚举类来定义了54张扑克牌，PokerType这个枚举类定义了斗地主中所有的牌型，其中None则表示不是符合规则的牌型。PokerHelper则是提供了所有工具方法。PokerCard中的每张牌满足（3+枚举值/4）=牌码，emmm，牌码就是指3、4、5、6....10、11、12、13、14、15、16、17。其中11、12、13、14、15、16、17分别代表J、Q、K、A、2、小王、大王。

##  洗牌算法

#### 随机洗牌算法

​    洗牌算法采用的是增量随机算法思想，这种思想模拟了真实玩牌中随着玩的次数增多，牌的随机性就越大。在PokerHelper中先预制了一副牌，洗牌时就是在这副牌上不断随机交换的过程。洗牌算法用的是叫做Fisher-Yates 洗牌算法。源码中的CARDS就是预制的54张扑克牌。

```c#
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
```

#### 不洗牌算法

不洗牌算法我的实现思路是先将炸弹牌（即每四张牌）进行随机交换，交换次数是随机的；然后再单张牌进行随机交换，交换次数也是随机的，最后再一次性每人发17张牌，而不是一张一张的发。

```C#
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
```



## 算法核心

### 如何判断牌型

​    这儿我们先从如何判断出每种牌型开始。

#### 单牌

​    要判断是否是单牌和简单只要满足当前出牌的数量等于1即可。

#### 对子

   判断对子也很简单，只要判断出牌数等于2同时两张牌的牌码相等。

#### 顺子

   从这儿开始就比较难懂了，注意理解哦。我们直接上源码进行解读算法思想。

```cs
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
```

第一步：牌数至少需要5张才可能组成顺子牌型；

第二步：依次求出每张牌的牌码，如果牌码大于14则一定不是顺子了（因为顺子不可能包含2、小王、大王）；

第三步：将每个牌码记录到一个临时的int值（代码中的tmp）中，只需要左移这个牌码数位即可；

第四步：如果这个牌是顺子，那么在tmp中的顺序是连续的，比如：0011111100这种，同时不可能存在相同的牌码，因此先“1<<当前牌的牌码”，这个值与tmp进行与操作，一定要等于0，如果等于1那么说明之前已经存在过相同的牌码的牌了，就一定不可能是顺子了；

第五步：记下顺子的最小牌码值；

第六步：遍历完所有的牌后检查tmp中的1是否是连续的，只需要从将tmp值右移(min+cards.Count)位，这一步如果你无法理解，可以画一下顺子和一个非顺子他们在int中的排列，如：3/4/5/6/7这个顺子的排列位：11111000，3/4/5/6/8这给非顺子的排列为101111000，需要右移动的位数为：3（最小值）+5（牌数）=8，分别右移动8位后，不是顺子的牌一定不等于0.（其它情况也都是一样的）。

#### 连对

​    找出最小牌码和最大牌码，以及累加所有牌码，使用等差数列求和公式直接判断。（顺子其实也可以用这种办法）

#### 王炸

   牌数一定等于2同时包含大小王。

#### 炸弹、三（四）带x、三（四）带x对的牌型判断

​    将这儿之前，先来看看333、6666这种有相同牌码值的特点。如果把他们都写入到一个int值中，那么只有一个位上为1。因此我们只要统计位为1的次数即可，同时又要把那些没有相同牌码的数不要进行统计。这就还需要一个值进来进行记录。先看源码，再一步一步讲解：

```cs
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
```

第一步：使用一个count值来表示总共要统计次数（这个值外部传递）；

第二步：将所有牌码记录到r这个int值中；

第三步：如果再记录牌码之前，r这个位已经为1了，那么将这个牌码记录到n这个值中；

第四步：判断r和n中当前牌码位是否是1，将这个值分别记录到p和k中，再使用count值减去(p+k)；当k==1时还要将n中的code位置为0，因为n是用来统计次数的，每次只能统计出1次。



有了这个算法后，后面要判断什么三带、四带或者不带简直不要太简单，如果是三带一，首先牌数一定等于4，其次count值一定等于3；三带一对：牌数一定等于5同时count值一定等于4。即：x带y对，那么count=x+y（x>=3）；x带y，那么count=x；如果x=4同时y=0那么这就是炸弹。

#### 飞机

   要看飞机这种牌型，就要先看飞机这种牌型的特点，先看什么都不带的：333/444，如果将牌码写入到int值中那么一定是连续的，因此在这一步的基础上再统计出为1的次数就ok了。思想和前面三带、四带还是很像的。OK，先看源码，再讲解：

```cs
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
```

可以看到除了同前面那样要统计出次数外，还有统计牌码相同而且数量等于3的牌的牌码的最小值和最大值，之后判断这些位是否连续。

有了这个算法基础后，飞机带y和飞机带y对，这种牌型都很好判断了；飞机不带，即y=0，那么其牌数一定是3的整数倍同时牌数大于等于6，之后count值=牌数；飞机带y张单牌时（2<=y<=5）时其count=y*3；飞机带y对时（2<=y<=4)，count=y*3+y。

### 如何判断玩家出牌是否符合规则

   这儿展示Check()算法的源码，这个算法需要指定当前玩家出牌的类型和上家出牌的类型，上家出牌的类型往往是可以预先指定的，因为网络同步时最好把这个值也发过去，减少重复计算。

```cs
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
```

   还有一种FastCheck()算法，则会个算法是Check()算法的快了20倍不止，FastCheck()算法的核心思想就是先使用上家出牌的类型预测当前玩家出牌的类型，这样可以不用获取当前玩家出牌的类型就能进行比较。这个算法大家看源码吧。快的原因就是GetPokerType()函数开销大（因为要使用Check()算法必须先知道当前玩家出牌的类型），FastCheck()算法就规避了这一点，因此很快。如果上家中没有一个人出牌呢，是不是只要当前玩家出牌的牌型是符合规则的就可以了，所有必须要有GetPokerType()函数。

#### 上家出的牌与当前玩家出的牌是同一类型时

   是同一类型那么其牌数一定是相等的，对于单牌、顺子、对子这种牌我们只需要比较所有牌的和的大小即可，符合规则那么一定是当前玩家出的牌的和大于上家；

   此外的其它类型都只要比较牌码相同且数量等于3或4的这些牌的求和大小即可。这一步怎么做呢？先看源码再做讲解：

```cs
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
```

可以看见呀，使用sum这个int值来统计的，当r的第code位为1时，将此值记录到n中，当n中的第code位为1时记录到sum中，这样sum就只会统计到牌码相同且数量大于等于3的牌。为什么只统计sum呢，因为三带、四带、飞机都是比较前面那些相同牌码且数量大于等于3的牌的大小呀！

#### 不是同一类型时

   这是最简单的，不是同一类型，那么当前玩家出牌想要符合规则一定是炸弹牌型同时牌型的枚举值大于上家。



## 提示算法

### 分析当前玩家手牌的组成情况

首先要将当前玩家的手牌组成情况分析出来，即：单牌、顺子、对子、连对、三顺、飞机、炸弹、王炸。这些牌型，由于54张牌用位来进行记录也就54位就够了，因此只需要一个long类型（64位就可以分析出玩家的所有牌型）。先看源码，再讲解算法。

```C#
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
```

首先将玩家的牌都记录到一个long类型的codes信息中去（左移每个牌的枚举值位），之后将这个值与一个long类型的bomb进行与操作，这个bomb是炸弹，即如果是4个3就是1111，转为十进行就是15，如果将这个值左移4位就能得到11110000，这个值刚好是4个4组成的位信息。现在要统计每种类型的牌有多少张，只需要将codes与bomb作与操作得到一个long类型的temp值，再统计这个temp值中1的次数，就知道这种类型的牌有多少张了，比如：codes=1110_0101_0000（玩家手牌为黑桃4，方块4、以及3给5，没有黑桃5），如果将这个值与bomb=0000_1111_0000（即4个4）相与，即temp=codes & bomb=0000_0101_0000，再统计temp中位为1的数量，即2个1，说明当前玩家的手牌中有两张4。因此后面只要1的数量为1就记录到单牌中（如果这个牌码值还小于15就要记录到顺子中）、为2就记录到对子中（如果这个牌码值还小于15就要记录到连对中）、为3就记录到三顺和飞机中，为4就记录到炸弹中。由于王炸比较特殊，即第52位和第56位为1就是王炸，因此只要判断codes的第52位和第56位是否都为1就可以指定玩家手牌中是否有王炸。

有了上面的记录后，还要单独将飞机、顺子、连对拆出来，因为单牌中也记录了顺子的牌，飞机中还记录了三顺的牌、对子中还记录了连对的牌。要拆出来也和简单。先看下面的拆解算法：

```C#
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
```

首先确定要连续几次才进行拆除，比如：顺子要5次，飞机要2次、连对要3次。举例说明这个算法的思想：

现在给的单牌codes=1000_0100_0000_0010_0100_1000_0100_0010（从左往右依次为红桃3、梅花4、方块5、梅花6、红桃7，梅花9和方块10），要从这里面把不能组成顺子的牌去掉（即梅花9和方块10），将codes与4个3组成的bomb值相遇得到temp值，如果temp值大于0，说明当前codes中有3这个牌，此时count值加1，那么从此次开始依次像后面进行统计，再将codes与4个4相与，还是大于0，则count值再加1，继续。直到等于0，当等于0后，如果count值大于等于5，说明这个区域的牌组成的是顺子。按此法，当count值小于5时，说明这个区域的牌不是顺子，则将这个区域的位全部置为0。按此法就能从单牌中得出顺子的牌了。得到顺子的牌后，再将这个顺子的牌与原来的单牌进行异或，异或的结果就是不包含顺子的牌的单牌了。

对于飞机和连对都是同样的道理，只是count值不一样而已。

下面是如何统计出一个long值中位为1的数量的算法：

```C#
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
```



### 上家有人出牌的情况

从当前玩家的手牌获取到提示出的牌，可以先分为两张情况进行讨论：①当前玩家的手牌数少于上家的出牌数；②当前玩家的手牌数多于或等于上家的出牌数。从这两种情况中可以先进行优先出与上家同一类型的牌，如果没有那么则当前玩家只有可能出炸弹。

##### 当前玩家的手牌数少于上家的出牌数

直接看源码吧，源码的注释很全。

```C#
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
```



##### 当前玩家的手牌数多于或等于上家的出牌数

优先出与上家牌型一致的牌。

将算法前先看几个封装的工具方法。

```C#
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
```

从long类型记录的牌的信息中读取到PokerCard的值的工具方法。

```C#
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
```

获取连续的牌中的最小牌码和最大牌码，如：3456这个四个连续的牌中获取最小的牌码为3最大为6。334455最小为3最大为5。（这个函数会在顺子、连对、飞机的提示算法中用到）

```C#
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

```

获取3334中的3这个牌码，444423中4这个牌码、即获取x带y或x带y对中x的值的工具函数。（会在三带、四带中用到）

```C#
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

```



###### 单牌

只要从当前玩家的单牌的信息中找到一个比上家出的单牌的牌码值大即可。

###### 对子

只要从当前玩家的对子牌的信息中找到一个比上家出的对子牌的牌码值大即可。

###### 顺子

只要从当前玩家的顺子牌的信息中找到一个与上家的顺子牌的牌数一样多同时当前玩家的顺子的最小的牌的牌码值比上家的大即可满足。顺子比较有三张情况：交叉、包含、完全不相交。如：34567和3456789属于包含关系，56789和678910JQ属于交叉关系，34567和8910JQKA属于完全不相交的关系。

###### 连对

提示原理和顺子是一样的。

###### 飞机

飞机由于要判断带翅膀和不带翅膀的情况，如果带翅膀则要先判断当前玩家的手牌是否能够组成与上家的飞机一致的翅膀，如：上家飞机带的是3张单牌，那么当前玩家也要带3张单牌才行。带翅膀的情况比较复杂，这儿讲解感觉很麻烦，如果你感兴趣建议读源码吧。就是凑翅膀的过程要综合考虑很多情况。

不带翅膀的比较原理和顺子一致的；对于带翅膀的，先判断翅膀能否凑足，翅膀凑足后再判断飞机不带翅膀的，之后再组合在一起。

###### 炸弹

炸弹的提示原理和单牌、对子都是一样的。



如果找不到与上家一致的牌型出，那么当前玩家只能出炸弹了。



### 上家没人出牌的情况

一种较为简单的提示出牌逻辑为：单牌>顺子>连对>对子>飞机>三顺>炸弹>王炸

```C#
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
                var feiJi = GetContinusMinMax(3, ref result.Item4);
                ReadPokerCard(feiJi.Item1, feiJi.Item2, ref result.Item4, tipCards);
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
```

