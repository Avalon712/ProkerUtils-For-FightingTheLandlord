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

​    洗牌算法采用的是增量随机算法思想，这种思想模拟了真实玩牌中随着玩的次数增多，牌的随机性就越大。在PokerHelper中先预制了一副牌，洗牌时就是在这副牌上不断随机交换的过程。洗牌算法用的是叫做Fisher-Yates 洗牌算法。源码中的CARDS就是预制的54张扑克牌。

```cs
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

第六步：遍历完所有的牌后检查tmp中的1是否是连续的，只需要从将tmp值右移(min+cards.Count)位，这一步如果你无法理解，可以画一下顺子和一个非顺子他们在int中的排列，如：3/4/5/6/7这个顺子的排列位：11111000，3/4/5/6/8这给非顺子的排列为101111000，需要右移动的位数为：3（最小值）+5（牌数）=8，分别右移动8位后，不是顺子的牌一定不等于8.（其它情况也都是一样的）。

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