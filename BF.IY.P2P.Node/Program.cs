// See https://aka.ms/new-console-template for more information
using BF.IY.Common;
using BF.IY.Common.Model;
using BF.IY.P2P.Node;
using Pastel;
using System.Drawing;
using System.Net.Sockets;


AppDriver driver = new AppDriver();
await driver.StartAuction();

"All Done... Press enter to exit".Pastel(Color.Green).PastelBg(Color.WhiteSmoke);
Console.ReadLine();
