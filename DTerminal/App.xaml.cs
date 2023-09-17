﻿using DTerminal.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DTerminal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IServiceCollection ServiceCollection = new ServiceCollection();
        public static IServiceProvider? ServiceProvider { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var views = System.Reflection.Assembly.GetExecutingAssembly().
                GetTypes().
                Where(x => x.Namespace == "DTerminal.Views" && x.GetCustomAttribute<ObsoleteAttribute>() is null).ToArray();
            var viewmodels = System.Reflection.Assembly.GetExecutingAssembly().
                GetTypes().
                Where(x => x.Namespace == "DTerminal.ViewModels" && x.GetCustomAttribute<ObsoleteAttribute>() is null).ToArray();

            foreach (var t in views)
            {
                ServiceCollection.AddScoped(t);
            }
            foreach (var t in viewmodels)
            {
                ServiceCollection.AddScoped(t);
            }
            App.ServiceProvider = ServiceCollection.BuildServiceProvider();
        }

        public App()
        {
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            MessageBox.Show("Task线程异常：" + e.Exception.Message);
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出      
                MessageBox.Show("UI线程异常:" + e.Exception.Message);
            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                MessageBox.Show("UI线程发生致命错误！" + ex.Message);
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("非UI线程发生致命错误");
            }
            sbEx.Append("非UI线程异常：");
            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            MessageBox.Show(sbEx.ToString());
        }


    }
}
