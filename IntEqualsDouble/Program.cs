using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IntEqualsDouble
{
    class Program
    {
        static readonly string SaveTempPath = "save.tmp";

        struct TaskInfo
        {
            public int begin;
            public int span;
        }
        static void Main()
        {
            List<int> badList = new List<int>();//存储不符合检测预期的值得列表
            int taskMax = 128;//其实可以根据当前运算速度，适时调整线程和线程任务量来优化速度，但是我懒
            int spanPerTime = 1024;
            Dictionary<Task<bool[]>, TaskInfo> tasks = new Dictionary<Task<bool[]>, TaskInfo>();
            int totalFinishedTask = 0;

            int InputPositiveInt32(string text)
            {
                bool flag = true;
                int inputInt = 0;
                string tempLine;
                while (flag)
                {
                    Console.Write(text);
                    tempLine = Console.ReadLine();
                    try
                    {
                        inputInt = int.Parse(tempLine);
                        if (inputInt > 0)
                        {
                            flag = false;
                        }
                        else
                        {
                            Console.WriteLine("Not a positive number.");
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Not a number.");
                    }
                }
                return inputInt;
            }
            int ClearTasks(int timeout)
            {
                int cmpTaskIdx;
                int cmpTaskCount = 0;

                while (tasks.Count > 0)
                {
                    if (timeout >= 0)
                    {
                        cmpTaskIdx = Task<bool[]>.WaitAny(tasks.Keys.ToArray(), timeout);
                        if (cmpTaskIdx == -1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        cmpTaskIdx = Task<bool[]>.WaitAny(tasks.Keys.ToArray());
                    }

                    Task<bool[]> cmpTask = tasks.Keys.ToArray()[cmpTaskIdx];
                    TaskInfo cmpTaskInfo = (from val in tasks
                                            where val.Key == cmpTask
                                            select val).Single().Value;
                    bool[] cmpTaskResult = cmpTask.Result;

                    CkResult(ref cmpTaskResult, cmpTaskInfo.begin, ref badList);
                    totalFinishedTask++;
                    cmpTaskCount++;

                    tasks.Remove(cmpTask);
                }
                Console.WriteLine("Clear Cmp; badList.Count={0:D10}", badList.Count);
                return cmpTaskCount;
            }

            //####从这开始###

            int i = 0;
            TaskInfo arg;
            DateTime spdScan0 = DateTime.Now, spdScan1;

            if ((new FileInfo(SaveTempPath)).Exists)
            {
                FileStream fs = new FileStream(SaveTempPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                EasyFile ef = new EasyFile(ref fs);
                taskMax = ef.ReadInt32();
                spanPerTime = ef.ReadInt32();
                totalFinishedTask = ef.ReadInt32();
                int badList_n = ef.ReadInt32();
                for(int j = 0; j < badList_n; j++)
                {
                    badList.Add(ef.ReadInt32());
                }
                fs.Close();
                i = (int)(int.MinValue + (long)totalFinishedTask * spanPerTime);
            }
            else
            {
                taskMax = InputPositiveInt32("taskMax = ");
                spanPerTime = InputPositiveInt32("spanPerTime = ");
                i = int.MinValue;
            }

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            bool ConsoleEventCallback(int eventType)
            {
                //https://docs.microsoft.com/en-us/windows/console/handlerroutine?redirectedfrom=MSDN
                const int CTRL_CLOSE_EVENT = 2;
                if (eventType == CTRL_CLOSE_EVENT && needProtect)
                {
                    FileStream fs = new FileStream(SaveTempPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    EasyFile ef = new EasyFile(ref fs);
                    ef.WriteInt32(taskMax);
                    ef.WriteInt32(spanPerTime);
                    ef.WriteInt32(totalFinishedTask);
                    ef.WriteInt32(badList.Count);
                    for (int j = 0; j < badList.Count; j++)
                    {
                        ef.WriteInt32(badList[j]);
                    }
                    fs.Close();
                }
                return false;
            }

            while(true)
            {
                if ((long)int.MaxValue - i < spanPerTime)
                {
                    break;
                }

                if (tasks.Count >= taskMax)
                {
                    int cmpTaskCount = ClearTasks(1);
                    spdScan1 = DateTime.Now;
                    Console.WriteLine("#Now Speed: {0} task/s; {1} number/s",
                        cmpTaskCount / spdScan1.Subtract(spdScan0).TotalMilliseconds * 1000,
                        cmpTaskCount * spanPerTime / spdScan1.Subtract(spdScan0).TotalMilliseconds * 1000
                        );
                    spdScan0 = spdScan1;
                }
                arg = new TaskInfo
                {
                    begin = i,
                    span = spanPerTime
                };
                tasks.Add(Task<bool[]>.Factory.StartNew(new Func<object, bool[]>(Cal), arg), arg);
                needProtect = true;

                i += spanPerTime;
            }
            //测算特殊范围
            if (tasks.Count >= taskMax)
            {
                ClearTasks(1);
            }
            needProtect = false;//懒得改成计数完成到哪一步，所以这里关闭保护，避免出错
            arg = new TaskInfo
            {
                begin = i,
                span = int.MaxValue - i
            };
            tasks.Add(Task<bool[]>.Factory.StartNew(new Func<object, bool[]>(Cal), arg), arg);
            ClearTasks(-1);
            //单独计算int.MaxValue
            arg = new TaskInfo
            {
                begin = int.MaxValue,
                span = 1
            };
            if (!Cal(arg)[0])
            {
                badList.Add(int.MaxValue);
            }

            //输出结果
            Console.WriteLine("badList[{0}]:", badList.Count);
            foreach (int temp in badList)
            {
                Console.Write("{0}, ", temp);
            }
            Console.WriteLine("!!!This program work with .NET Core 3.1!!!");

            Console.ReadLine();
        }

        private static bool needProtect = false;
        //https://stackoverflow.com/questions/4646827/on-exit-for-a-console-application{
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        //}

        private static bool[] Cal(object argument)
        {
            DateTime beginDT = DateTime.Now;
            TaskInfo taskInfo = (TaskInfo)argument;
            bool[] rs = new bool[taskInfo.span];

            double d;
            int i, val;
            for (i = 0; i < taskInfo.span; i++)
            {
                //计算内容
                val = taskInfo.begin + i;
                d = double.Parse(String.Format("{0}.0", val));
                rs[i] = val == d;
            }

            //StringBuilder sb = new StringBuilder("");
            int falseCount = 0;
            foreach (bool r in rs)
            {
                //sb.Append(r ? "_" : "!");
                if (!r)
                {
                    falseCount++;
                }
            }
            DateTime endDT = DateTime.Now;
            Console.WriteLine("[{0}~{1}]{2:X8}: {3:D10} ~ {4:D10} [{5:D10}]",
                String.Format("{0}.{1:D3}", beginDT.ToString("HH:mm:ss"), beginDT.Millisecond),
                String.Format("{0}.{1:D3}", endDT.ToString("HH:mm:ss"), endDT.Millisecond),
                Thread.CurrentThread.ManagedThreadId,
                taskInfo.begin,
                taskInfo.begin + taskInfo.span - 1,
                falseCount
                );

            return rs;
        }

        private static void CkResult(ref bool[] results, int beginIdx, ref List<int> badList)
        {
            int i;
            for (i = 0; i < results.Length; i++)
            {
                if (!results[i])
                {
                    badList.Add(beginIdx + i);
                }
            }
        }
    }
}
