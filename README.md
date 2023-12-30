# BF Auction Test Project
### Description
This is a sample implementation of building a P2P networking using .NET 8 gRPC. In this project user can run multiple clients and communicate with its connected peers. 
You can run the client from your IDE or by going into main project directory `BFA.IY.App\BF.IY.P2P.Node`
Once you are in the required directory, please give following command
`dotnet run BF.IY.P2P.Node.csproj`
To start another node, please repeat the above step in another console/terminal window.

On the start of a client/client, it will ask you to provide 
	- Node Name
	- Node Port
	- To clear service registry or not
It is very important that for fresh start and for very first node, you clear the service registry. I am using SQLite DB (`auction_clients.sqlite`) for node registration and discovery. This DB is placed in`BFA.IY.App\BF.IY.P2P.Node` directory and nodes refer to this DB by using relative path.

Once all nodes are started they will discover and connect with each other and Auction menu will be shown to user. From there you can create new Auction and Bid on existing Auctions. 
Important rules are that:
	- Node can only bid on Auction which is created by some other Node
	- Node can accept only Bid for Auction which it has created itself.

#### Missing & Areas of Improvements
I tried my best to cover most of aspects in very limited time. But these things are missing or need improvements:
	- Better UI & Error Handling for Inputs
	- Graceful Exit
	- Proper Transactions
	- Project Structure & Naming
	- Proper Service Discovery Mechanism

#### Screenshots
![Image1](https://github.com/imyounas/BFA.IY.App/blob/master/2.png)
![Image2](https://github.com/imyounas/BFA.IY.App/blob/master/3.png)
![Image3](https://github.com/imyounas/BFA.IY.App/blob/master/4.png)
![Image4](https://github.com/imyounas/BFA.IY.App/blob/master/1.png)
