
namespace PokerUtils
{
    /// <summary>
    /// 斗地主中出牌的所有类型
    /// </summary>
    public enum PokerType
    {
        /// <summary>
        /// 非法出牌
        /// </summary>
        None,

        /// <summary>
        /// 单牌
        /// </summary>
        Single,

        /// <summary>
        /// 对子
        /// </summary>
        Double,

        /// <summary>
        /// 顺子
        /// </summary>
        ShunZi,

        /// <summary>
        /// 连对
        /// </summary>
        LianDui,

        /// <summary>
        /// 斗地主单出三张牌
        /// </summary>
        ThreeWithNone,

        /// <summary>
        /// 三带一
        /// </summary>
        ThreeWithOne,

        /// <summary>
        /// 三带一对
        /// </summary>
        ThreeWihtDouble,

        /// <summary>
        /// 四带二
        /// </summary>
        FourWithTwo,

        /// <summary>
        /// 四带一对
        /// </summary>
        FourWithDouble,

        /// <summary>
        /// 四带两对
        /// </summary>
        FourWithTwoDouble,

        /// <summary>
        /// 飞机什么都不带
        /// </summary>
        AeroplaneWithNone,

        /// <summary>
        /// 飞机带两张牌
        /// </summary>
        AeroplaneWithTwo,

        /// <summary>
        /// 飞机带三张单牌
        /// </summary>
        AeroplaneWithThree,

        /// <summary>
        /// 飞机带四张单牌
        /// </summary>
        AeroplaneWithFour,

        /// <summary>
        /// 飞机带五张单牌
        /// </summary>
        AeroplaneWithFive,

        /// <summary>
        /// 飞机带两对
        /// </summary>
        AeroplaneWithTwoDouble,

        /// <summary>
        /// 飞机带三对
        /// </summary>
        AeroplaneWithThreeDouble,

        /// <summary>
        /// 飞机带四对
        /// </summary>
        AeroplaneWithFourDouble,

        /// <summary>
        /// 炸弹
        /// </summary>
        Bomb,

        /// <summary>
        /// 王炸
        /// </summary>
        KingBomb,
    }
}
