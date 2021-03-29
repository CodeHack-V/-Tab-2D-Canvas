using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    class Helper
    {
        public static MainWindow mainWindow = null;
        public static int MoveSpeed = 1;//移动速度
        public static double MoveStep = 0.01;//移动步长
        public static bool Locker = true;//同步锁
        public static int History = 0;//历史点击按钮顺序
        public static bool Change = false;//Load时触发的SizeChange锁
        public static double Width = 0.00, Left = 0.00;//记录最后一次点击按钮的坐标
        public static void MainWindowEventBinding()
        {
            mainWindow.Page1.Click += PageButtonClick;
            mainWindow.Page2.Click += PageButtonClick;
            mainWindow.Page3.Click += PageButtonClick;
            mainWindow.Page4.Click += PageButtonClick;
            mainWindow.Page5.Click += PageButtonClick;
            mainWindow.Canvas_M.Width = (mainWindow.Width - 5 * 10) / 5;
            mainWindow.SizeChanged += (object sender, SizeChangedEventArgs e) =>
            {
                if (Change)
                {
                    mainWindow.Canvas_M.Width = (mainWindow.ActualWidth - 5 * 10) / 5;
                    object Left = mainWindow.FindName("Page" + History);//
                    if (Left != null)
                    {
                        Button button = (Button)Left;
                        double X = button.TranslatePoint(new Point(), mainWindow).X;
                        Canvas.SetLeft(mainWindow.Canvas_M, X);
                    }
                }
                else
                {
                    Change = true;
                }
            };
        }
        public static void CanvasShow()
        {
            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                if (Locker)
                {
                    Locker = false;
                    mainWindow.Canvas_M.Dispatcher.Invoke(() =>
                    {
                        mainWindow.Canvas_M.Width = Width;
                    });
                    double CM = 0.00;
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        CM = Canvas.GetLeft(mainWindow.Canvas_M);//Canvas Left
                    });
                    if (CM < Left)
                    {
                        while (CM < Left)
                        {
                            double CL;
                            if (Left - CM >= 1)
                            {
                                CL = CM + MoveStep;
                            }
                            else
                            {
                                CL = CM + (Left - CM);
                            }
                            mainWindow.Canvas_M.Dispatcher.Invoke(() =>
                            {
                                Canvas.SetLeft(mainWindow.Canvas_M, CL);
                            });
                            CM++;
                            Thread.Sleep(MoveSpeed);
                        }
                        Locker = true;
                    }
                    else
                    {
                        while (CM > Left)
                        {
                            double CL;
                            if (CM - Left >= 1)
                            {
                                CL = CM - MoveStep;
                            }
                            else
                            {
                                CL = CM + (CM - Left);
                            }
                            mainWindow.Canvas_M.Dispatcher.Invoke(() =>
                            {
                                Canvas.SetLeft(mainWindow.Canvas_M, CL);
                            });
                            CM--;
                            Thread.Sleep(MoveSpeed);
                        }
                        Locker = true;
                    }
                }
            });
        }
        public static void PageButtonClick(object sender, RoutedEventArgs e)
        {
            if (Locker)
            {
                Button CurrentButton = ((Button)sender);
                UIElementCollection Buttons = mainWindow.NavTop.Children;
                foreach (UIElement button in Buttons)
                {
                    ((Button)button).Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
                CurrentButton.Foreground = new SolidColorBrush(Color.FromRgb(33, 117, 188));
                History = Convert.ToInt32(CurrentButton.Name.Replace("Page", ""));
                Width = CurrentButton.ActualWidth;
                Left = CurrentButton.TranslatePoint(new Point(), mainWindow).X;
                mainWindow.PageMain.Navigate(new Uri(CurrentButton.Name + ".xaml", UriKind.Relative));
                CanvasShow();
            }
        }
    }
}