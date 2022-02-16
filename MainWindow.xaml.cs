//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: MainWindow.xaml.cs
//
//--------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiningPhilosophers
{
    using Philosopher = Ellipse;
    using Fork = BufferBlock<bool>;

    public partial class MainWindow : Window
    {
        private const int NUM_PHILOSOPHERS = 5;
        private const int TIMESCALE = 1000;
        private readonly Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the philosophers and forks
            var philosophers = new Philosopher[NUM_PHILOSOPHERS];
            var forks = new Fork[NUM_PHILOSOPHERS];
            for (int i = 0; i < philosophers.Length; i++)
            {
                diningTable.Children.Add(philosophers[i] = new Philosopher { Height = 75, Width = 75, Fill = Brushes.Red, Stroke = Brushes.Black });
                forks[i] = new Fork();
                forks[i].Post(true);
            }

            // Запустим философов в произвольном парядке
            for (int i = 0; i < philosophers.Length; i++)
            {
                // Pass the forks to each philosopher in an ordered (lock-leveled) manner
                RunPhilosopherAsync(philosophers[i],
                    i < philosophers.Length - 1 ? forks[i] : forks[1],
                    i < philosophers.Length - 1 ? forks[i + 1] : forks[i]);
            }
        }

        /// <summary>Runs a philosopher asynchronously.</summary>
        private async void RunPhilosopherAsync(Philosopher philosopher, Fork leftFolk, Fork rightFolk)
        {
            // Think, Wait, and Eat, ad infinitum
            while (true)
            {
                // Think (Yellow)
                philosopher.Fill = Brushes.Yellow;
                await Task.Delay(_rand.Next(10) * TIMESCALE);

                // Wait for forks (Red)
                philosopher.Fill = Brushes.Red;
                await leftFolk.ReceiveAsync();//ждёт когда придёт сообщение в канал вилка слева свободна
                await rightFolk.ReceiveAsync();//ждёт когда придёт сообщение в канал вилка вилка справа  свободна

                // Eat (Green)
                philosopher.Fill = Brushes.Green;
                await Task.Delay(_rand.Next(10) * TIMESCALE);

                // Done with forks; put them back
                leftFolk.Post(true);
                rightFolk.Post(true);
            }
        }
    }
}