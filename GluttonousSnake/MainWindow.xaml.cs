using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GluttonousSnake
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        int ROWS = 50, COLUMNS = 60;               //列数与行数
        Rectangle snake;      //蛇体
        int SIZE = 5;                               //蛇长度
        public DispatcherTimer snakeTimer;                 //游戏线程
        List<int> snakeIndex = new List<int>();       //记录所有蛇的索引
        List<int> orientation;        //操作方向及转折点,1:left,2:right,3:up,4:doown;row;column,以及某个方向包含蛇体的索引  
        List<List<int>> orientationList = new List<List<int>>();          //记录所有操作
        int orientationIndex;                               //需要执行的某个方向的索引
        bool canPlay = false;                                        //是否可以操作游戏
        int snakeHead;                                      //蛇的头部
        int score;                                    //分数
        double time;                                    //时间
        Random location = new Random();                               //食物位置
        int row, col;                                            //食物的行与列
        bool foodEat = true;                                   //食物是否被吃
        bool snakeEat = false;                              //蛇是否吃到食物
        DispatcherTimer foodTimer;                  //食物线程
        bool isCreate;                              //食物是否已经创建成功
        //播放背景音乐
        //SoundPlayer sp = new SoundPlayer(Properties.Resources.ResourceManager.GetStream("GluttonousSnake"));
        //Random color = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region 开始游戏
        public void start()
        {
            snakeIndex.Clear();
            //初始化方向
            orientationList.Clear();
            orientation = new List<int>();
            orientation.Add(2);           //方向
            orientation.Add(0);           //转折行
            orientation.Add(0);           //转折列

            //初始化蛇
            gameGrid.Children.Clear();
            for (int i = 0; i < SIZE; i++)
            {
                snake = new Rectangle();
                snake.Fill = new SolidColorBrush(Colors.White);
                //snake.Stroke = new SolidColorBrush(Colors.White);
                //snake.StrokeThickness = 1;
                //snake.Fill =new SolidColorBrush(Color.FromRgb((byte)color.Next(1, 256), (byte)color.Next(1, 256), (byte)color.Next(1, 256)));
                Grid.SetColumn(snake, i);
                gameGrid.Children.Add(snake);
                snakeIndex.Add(gameGrid.Children.IndexOf(snake));
                orientation.Add(i);
            }
            orientationList.Add(orientation);
            canPlay = true;
            score = 0;
            time = 0;

            foodEat = true;
            foodTimer.Start();
            stopButton.IsEnabled = true;
            snakeTimer.IsEnabled = true;
            stateLabel.Content = "进行中";
            snakeTimer.Start();
            //sp.PlayLooping();
        }
        #endregion

        #region 暂停游戏
        public void stop()
        {
            if (stopButton.Content.ToString() == "暂停")
            {
                snakeTimer.Stop();
                canPlay = false;
                stopButton.Content = "继续";
                stateLabel.Content = "暂停中";
                foodTimer.Stop();
                //sp.Stop();
            }
            else
            {
                stopButton.Content = "暂停";
                snakeTimer.Start();
                stateLabel.Content = "进行中";
                foodTimer.Start();
                //sp.PlayLooping();
            }
        }
        #endregion

        #region 开始按钮
        private void start_Click(object sender, RoutedEventArgs e)
        {
            start();
        }
        #endregion

        #region 暂停按钮
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            stop();
        }
        #endregion

        #region 激活主窗体被取消
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (snakeTimer.IsEnabled)
            {
                stop();
            }
        }
        #endregion

        #region 初始化游戏环境
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //加载列
            for (int i = 0; i < COLUMNS; i++)
            {
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //加载行
            for (int i = 0; i < ROWS; i++)
            {
                gameGrid.RowDefinitions.Add(new RowDefinition());
            }
            //加载游戏线程
            snakeTimer = new DispatcherTimer();
            snakeTimer.Interval = TimeSpan.FromMilliseconds(600 - speedSlider.Value);
            snakeTimer.Tick += snakeTimer_Tick;

            this.KeyDown += new KeyEventHandler(game_KeyDown);
            InputMethod.SetIsInputMethodEnabled(this, false);
            stopButton.IsEnabled = false;
            snakeTimer.IsEnabled = false;
            stateLabel.Content = "未开始";

            //食物线程
            foodTimer = new DispatcherTimer();
            foodTimer.Interval = snakeTimer.Interval;
            foodTimer.Tick += foodTimer_Tick;
        }
        #endregion

        #region 游戏运行中
        public void snakeTimer_Tick(object sender, EventArgs e)
        {
            if (isGameOver())
            {
                stateLabel.Content = "已结束";
                foodTimer.Stop();
                snakeTimer.IsEnabled = false;
                canPlay = false;
                stopButton.IsEnabled = false;
                //sp.Stop();
                MessageBox.Show("你玩完啦！", "贪吃蛇", MessageBoxButton.OK);
                return;
            }
            if (snakeEat)
            {
                snakeEat = false;
                return;
            }
            //更新分数与时间
            time += snakeTimer.Interval.TotalMilliseconds / 1000;
            timeLabel.Content = time.ToString("#0.000");
            scoreLabel.Content = score;
            //更新速度
            snakeTimer.Interval = TimeSpan.FromMilliseconds(600 - speedSlider.Value);
            //检测食物是否被吃掉
            orientationIndex = orientationList.Count - 1;
            //计算左
            if (orientationList[orientationIndex][0] == 1)
            {
                if (Grid.GetRow(gameGrid.Children[snakeIndex[snakeHead]]) == row
                    && (Grid.GetColumn(gameGrid.Children[snakeIndex[snakeHead]]) - 1 == col))
                {
                    foodEat = true;
                }
            }
            //计算右
            else if (orientationList[orientationIndex][0] == 2)
            {
                if (Grid.GetRow(gameGrid.Children[snakeIndex[snakeHead]]) == row
                    && (Grid.GetColumn(gameGrid.Children[snakeIndex[snakeHead]]) + 1 == col))
                {
                    foodEat = true;
                }
            }
            //计算上
            else if (orientationList[orientationIndex][0] == 3)
            {
                if ((Grid.GetRow(gameGrid.Children[snakeIndex[snakeHead]]) - 1 == row)
                    && (Grid.GetColumn(gameGrid.Children[snakeIndex[snakeHead]]) == col))
                {
                    foodEat = true;
                }
            }
            //计算下
            else if (orientationList[orientationIndex][0] == 4)
            {
                if ((Grid.GetRow(gameGrid.Children[snakeIndex[snakeHead]]) + 1 == row)
                    && (Grid.GetColumn(gameGrid.Children[snakeIndex[snakeHead]]) == col))
                {
                    foodEat = true;
                }
            }
            if (foodEat)
            {
                score += 10;
                snakeEat = true;
                snakeHead++;
                snakeIndex.Add(snakeHead);
                orientationList[orientationIndex].Add(snakeHead);
            }
            //移动蛇
            //orientationIndex = orientationList.Count - 1;
            for (int j = orientationIndex; j >= 0; j--)
            {
                for (int i = orientationList[j].Count - 1; i >= 3; i--)
                {
                    //左
                    if (orientationList[j][0] == 1)
                    {
                        Grid.SetColumn(gameGrid.Children[orientationList[j][i]], Grid.GetColumn(gameGrid.Children[orientationList[j][i]]) - 1);
                    }
                    //右
                    else if (orientationList[j][0] == 2)
                    {
                        Grid.SetColumn(gameGrid.Children[orientationList[j][i]], Grid.GetColumn(gameGrid.Children[orientationList[j][i]]) + 1);
                    }
                    //上
                    else if (orientationList[j][0] == 3)
                    {
                        Grid.SetRow(gameGrid.Children[orientationList[j][i]], Grid.GetRow(gameGrid.Children[orientationList[j][i]]) - 1);
                    }
                    //下
                    else if (orientationList[j][0] == 4)
                    {
                        Grid.SetRow(gameGrid.Children[orientationList[j][i]], Grid.GetRow(gameGrid.Children[orientationList[j][i]]) + 1);
                    }
                    canPlay = true;
                    //将转折点从一个方向移到另一个方向
                    if (j < orientationIndex)
                    {
                        if (Grid.GetRow(gameGrid.Children[orientationList[j][i]]) == orientationList[j + 1][1] &&
                            Grid.GetColumn(gameGrid.Children[orientationList[j][i]]) == orientationList[j + 1][2])
                        {
                            orientationList[j + 1].Add(orientationList[j][i]);
                            orientationList[j].RemoveAt(i);
                        }
                    }
                }
                //删除最后一个方向，如果没有元素
                if (j == 0 && orientationList[j].Count == 3)
                {
                    orientationList.RemoveAt(j);
                    return;
                }
            }
        }
        #endregion

        #region 键盘操作
        public void game_KeyDown(object sender, KeyEventArgs e)
        {
            orientationIndex = orientationList.Count - 1;
            //下
            if (canPlay && (e.Key == Key.Down || e.Key == Key.S))
            {
                if (orientationList[orientationIndex][0] == 4 || orientationList[orientationIndex][0] == 3)
                {
                    return;
                }
                addDirection(4);
            }
            //上
            else if (canPlay && (e.Key == Key.Up || e.Key == Key.W))
            {
                if (orientationList[orientationIndex][0] == 4 || orientationList[orientationIndex][0] == 3)
                {
                    return;
                }
                addDirection(3);
            }
            //左
            else if (canPlay && (e.Key == Key.Left || e.Key == Key.A))
            {
                if (orientationList[orientationIndex][0] == 1 || orientationList[orientationIndex][0] == 2)
                {
                    return;
                }
                addDirection(1);
            }
            //右
            else if (canPlay && (e.Key == Key.Right || e.Key == Key.D))
            {
                if (orientationList[orientationIndex][0] == 1 || orientationList[orientationIndex][0] == 2)
                {
                    return;
                }
                addDirection(2);
            }
            //start
            else if (e.Key == Key.Z)
            {
                start();
            }
            //stop
            else if (stopButton.IsEnabled && e.Key == Key.X)
            {
                stop();
            }
            //加速
            else if (e.Key == Key.R)
            {
                speedSlider.Value += 5;
            }
            //减速
            else if (e.Key == Key.E)
            {
                speedSlider.Value -= 5;
            }
            //显示网格
            else if (e.Key == Key.Q)
            {
                if (showGridLinesCheckBox.IsChecked == true)
                {
                    showGridLinesCheckBox.IsChecked = false;
                }
                else
                {
                    showGridLinesCheckBox.IsChecked = true;
                }
            }
        }
        #endregion

        #region 增加方向
        public void addDirection(int direction)
        {
            snakeHead = snakeIndex.Count - 1;
            orientation = new List<int>();
            orientation.Add(direction);
            orientation.Add(Grid.GetRow(gameGrid.Children[snakeHead]));
            orientation.Add(Grid.GetColumn(gameGrid.Children[snakeHead]));
            orientation.Add(snakeHead);
            orientationList[orientationIndex].RemoveAt(orientationList[orientationIndex].LastIndexOf(snakeHead));
            orientationList.Add(orientation);
            //防止频繁按键
            canPlay = false;
        }
        #endregion

        #region 检测游戏是否结束
        public bool isGameOver()
        {
            snakeHead = snakeIndex.Count - 1;
            bool over = (Grid.GetRow(gameGrid.Children[snakeHead]) == 0 && orientationList[orientationList.Count - 1][0] == 3)
                || Grid.GetRow(gameGrid.Children[snakeHead]) > 49
                || (Grid.GetColumn(gameGrid.Children[snakeHead]) == 0 && orientationList[orientationList.Count - 1][0] == 1)
                || Grid.GetColumn(gameGrid.Children[snakeHead]) > 59;
            if (over)
            {
                return true;
            }
            for (int i = 0; i < snakeIndex.Count - 1; i++)
            {
                for (int j = i + 1; j < snakeIndex.Count; j++)
                {
                    if (Grid.GetColumn(gameGrid.Children[i]) == Grid.GetColumn(gameGrid.Children[j])
                        && Grid.GetRow(gameGrid.Children[i]) == Grid.GetRow(gameGrid.Children[j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region 食物线程
        public void foodTimer_Tick(object sender, EventArgs e)
        {
            foodTimer.Interval = snakeTimer.Interval;
            //创建食物
            if (foodEat)
            {
                snake = new Rectangle();
                snake.Fill = new SolidColorBrush(Colors.White);
                //snake.Stroke = new SolidColorBrush(Colors.White);
                //snake.StrokeThickness = 1;
                //snake.Fill = new SolidColorBrush(Color.FromRgb((byte)color.Next(1, 256), (byte)color.Next(1, 256), (byte)color.Next(1, 256)));
                while (true)
                {
                    row = location.Next(1, 50);
                    col = location.Next(1, 60);
                    for (int i = 0; i < snakeIndex.Count; i++)
                    {
                        if ((row != Grid.GetRow(gameGrid.Children[i])) && (col != Grid.GetColumn(gameGrid.Children[i])))
                        {
                            Grid.SetColumn(snake, col);
                            Grid.SetRow(snake, row);
                            gameGrid.Children.Add(snake);
                            isCreate = true;
                            foodEat = false;
                            break;
                        }
                    }
                    if (isCreate)
                    {
                        isCreate = false;
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
