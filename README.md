# VGMToolbox---source-code
 vgm工具箱汉化版


感谢Manicsteiner分享的源码，在他的源码基础上修改，花了两天的时间终于汉化好了，我本来以为昨天汉化的差不多了，谁知道我汉化的只是界面，还有很多隐藏的文本没有汉化，所以今天又弄了一天，头一次汉化这个工具，很多的cs文件看的我眼花缭乱。我在这个工具添加了多个xml文件，用于高级切割器/偏移量查找器，分别是wiiufwar.xml、wiiufwav.xml、RIFX大端序wem.xml、webp image.xml、wwise-pck-wsp-bnk.xml、Koei Tecmo kvs.xml、sony pcm.xml、dds image.xml、png image.xml、unity file共10个xml文件，前两个是原作者没有集成到vgm工具箱的，被我在某个网址找到了，随后自行添加了进去，该工具自带ktss.xml，这是用于光荣特库摩游戏的，缺一个kvs.xml，我简单修改了一下ktss.xml给添加进去了，再看webp image.xml和wwise-pck-wsp-bnk.xml，这两个是基于RIFF.xml修改来的，因为很多人不知道这个RIFF.xml是干嘛用的，所以我干脆给大家添加了个用于提取webp和wwise的wem的xml文件，这样大家一看就知道是做什么用的，接着是RIFX大端序wem.xml，这个是用于解xbox360平台大端序的音频的，RIFX文件头的音频实际上是大端序的wem，而RIFF是小端序，再就是dds image.xml和png image.xml，算上前边的webp image.xml，我已经把vgm工具箱变成了可以提取三种图片的解包工具，不过png和dds都需要输入终止字符串，并勾选设置为16进制，原本vgm工具箱终止字符串的输入窗口太窄，我现在已经弄宽了，然后把勾选设置为16进制的坐标往下移动了点，这样看起来舒服多了。最后就是unity file.xml，用于切割打包到一起的unity文件，比如江湖如梦、剑与远征这种unity游戏。

本工具实现了95%以上的汉化，大家可放心使用，程序是一边汉化一边调式，处理了好多次报错，文本汉化不是啥都能改，有的一改就完犊子，优先改text后面的英文和message后面的英文，还有大量的英文是在app.config这个配置文件里面的，这个最容易修改。

——偷吃布丁的涅普提努            2025/1/26
