using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GAT.Utils
{
    static class ConsoleControl
    {
        public static bool killApplication = false;
        public static void windowListener()
        {
            Console.WriteLine("To perform an action press the appropriate key.");
            printLine();
            Console.WriteLine("\tm = Market Order \n\tl = Limit Order \n\tc = Cancel an Order \n\tr = Replace Request");
            printLine();

            Console.Write("Awaiting action: ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();

            if (keyInfo.KeyChar == 'x')
            {
                killApplication = true;
            }

            //if (keyInfo.KeyChar == 'm') //market order
            //{
            //    printLine();
            //    Console.WriteLine("\nSending Market Order ");
            //    int id = rand.Next(100000000, 200000000);

            //    bool complete = gatherOrderInfo();

            //    if (!complete)
            //        continue;

            //    QuickFix42.NewOrderSingle msg;

            //    if (route == null)
            //        msg = maker.sendMarketOrder(id.ToString(), Program.symbol, Program.qty, Program.side);
            //    else
            //        msg = maker.sendMarketOrder(id.ToString(), Program.symbol, Program.qty, Program.side, route, route, channel);

            //    Session.sendToTarget(msg, oKnightSessionID);
            //    printLine();
            //}


            //else if (keyInfo.KeyChar == 'l') //limitOrder
            //{
            //    printLine();
            //    Console.WriteLine("\nSending Limit Order ");
            //    int id = rand.Next(100000000, 200000000);

            //    bool complete = gatherOrderInfo(true);

            //    if (!complete)
            //        continue;

            //    QuickFix42.NewOrderSingle msg;

            //    if (route == null)
            //        msg = maker.sendLimitOrder(id.ToString(), Program.symbol, Program.qty, Program.side, Program.price);
            //    else
            //        msg = maker.sendLimitOrder(id.ToString(), Program.symbol, Program.qty, Program.side, Program.price, route, channel);

            //    Session.sendToTarget(msg, oKnightSessionID);
            //    printLine();
            //}


            //else if (keyInfo.KeyChar == 'c') //cancel request
            //{
            //    printLine();
            //    int index;
            //    try
            //    {
            //        int lorderCount = handle.printLimitList();

            //        if (!(lorderCount > 0))
            //        {
            //            Console.WriteLine("\nNo Orders That can be canceled\n");
            //            continue;
            //        }

            //        Console.WriteLine("\nChoose the index of the order you wish to cancel.");
            //        Console.Write("Index (-1 to cancel): ");
            //        string sIndex = Console.ReadLine();

            //        index = int.Parse(sIndex);

            //        if (index < 0)
            //            continue;

            //        Console.WriteLine("\nSending CancelRequest on Order " + handle.getLimitOrderOriginalID(index) + "\n");
            //        int id = rand.Next(100000000, 200000000);

            //        QuickFix42.OrderCancelRequest msg = handle.generateCancelRequest(id.ToString(), index);

            //        Session.sendToTarget(msg, oKnightSessionID);

            //        handle.removeLimitOrder(index);
            //        printLine();
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.Message + "An Error occured please try again");
            //    }
            //}


            //else if (keyInfo.KeyChar == 'r')  //Replace request
            //{
            //    printLine();
            //    int index;
            //    Console.WriteLine("\nSending ReplaceRequest on Order " + OriginalClID + "\n");
            //    int id = rand.Next(100000000, 200000000);

            //    Console.WriteLine("Choose the index of the order you wish to cancel.");
            //    handle.printLimitList();
            //    Console.Write("Index (-1 to cancel): ");
            //    string sIndex = Console.ReadLine();

            //    index = int.Parse(sIndex);

            //    if (index < 0)
            //        continue;

            //    Console.WriteLine("Enter the change values (0 will keep original value)");

            //    bool complete = gatherReplaceInfo();

            //    if (!complete)
            //        continue;

            //    QuickFix42.OrderCancelReplaceRequest msg = handle.generateReplaceRequest(id.ToString(), Program.qty, Program.price, Program.stopx, index);

            //    Session.sendToTarget(msg, oKnightSessionID);
            //    printLine();
            //}
            //else if (keyInfo.KeyChar == 'f') //flush orders create and send an entered number of orders
            //{
            //    bool complete = gatherOrderInfo();

            //    if (!complete)
            //        continue;


            //    Console.Write("How many orders do you wish to send? ");
            //    String numberOfOrders = Console.ReadLine();
            //    int NOO = 0;

            //    if (Int32.TryParse(numberOfOrders, out NOO))
            //    {
            //        for (int i = 0; i < NOO; i++)
            //        {
            //            int id = rand.Next(100000000, 200000000);
            //            QuickFix42.NewOrderSingle msg = maker.sendMarketOrder(id.ToString(), Program.symbol, Program.qty++, Program.side);//, Program.price++);
            //            Session.sendToTarget(msg, oKnightSessionID);
            //            //Thread.Sleep(200);
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("Invalid Entry");
            //    }
            //}
            //else if (keyInfo.KeyChar == 's')
            //{
            //    Console.Write("\nSpecify your route: ");
            //    string route = Console.ReadLine();

            //    Console.Write("\nSpecify your Symbol: ");
            //    string sym = Console.ReadLine();

            //    int id = rand.Next(100000000, 200000000);

            //    QuickFix42.NewOrderSingle msg = maker.sendKnightMarketOrder(id.ToString(), route.ToUpper(), sym.ToUpper());
            //    Session.sendToTarget(msg, oKnightSessionID);
            //}
            //else if (keyInfo.KeyChar == 'x')//send manual cancel
            //{
            //    //int id = rand.Next(100000000, 200000000);

            //    //QuickFix42.OrderCancelRequest msg = handle.generateCancelRequest(id.ToString(), index);

            //    //Session.sendToTarget(msg, oKnightSessionID);

            //    ////handle.removeLimitOrder(index);
            //    //printLine();
            //}
            //else if (keyInfo.KeyChar == 'i')
            //{

            //}
            //else if (keyInfo.KeyChar == 'p')
            //{
            //    Console.Write("ClOrdID: " + OriginalClID);
            //}
            //else if (keyInfo.KeyChar == 'e')
            //{
            //    bool sent = Session.sendToTarget(DropCopy(), oKnightSessionID);

            //    if (sent)
            //        Console.WriteLine("Successfully Sent");
            //    else
            //        Console.WriteLine("Message was not delivered");
            //}
            Thread.Sleep(500);
        }
        
        public static void printLine()
        {
            Console.WriteLine("----------------------------------------");
        }
    }
}
