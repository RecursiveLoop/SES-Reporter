# SES Cross Account Reporting Tool
###### Written by Elgin Lam

### The Problem

There is currently no easy way to obtain Amazon SES statistics across multiple accounts.
It is possible to get these statistics for each account by calling the SES.GetSendStatistics() call, but if you have a lot of accounts, 
it's not easy to manually aggregate these calls across multiple accounts.

https://docs.aws.amazon.com/ses/latest/APIReference/API_GetSendStatistics.html


### What this does

It allows you to assume a role and loop through multiple accounts to gather Amazon SES statistics. It also goes through each of the regions that SES currently supports (N. Virginia, Oregon, Ireland) to gather statistics and saves all the statistics into a file.

Proudly written in a browser using [AWS Cloud9](https://aws.amazon.com/cloud9/)

### How to use the tool

1. Install .NET Core 2.0. This works on Windows, MacOS, Linux. https://www.microsoft.com/net/learn/get-started 

2. Pull the repository.

3. Restore the NuGet packages using 'dotnet restore' in the project's root.

4. Make sure the credentials to the primary account that has the trust is setup like here: https://docs.aws.amazon.com/sdk-for-net/v2/developer-guide/net-dg-config-creds.html 

5. Open up Accounts.txt in the root and fill it in with the account numbers that you want to run this tool in. These accounts need to have a role setup that you can assume into, and the role needs to have the SES read permissions.

6. Build the application using 'dotnet build'

7. Run the application using 'dotnet run (Role Name)' with (Role Name) being the name of the role that you have allowed for cross account access.

8. The application should run and write the output to a text file.

### Why .NET Core?

I wanted to try something different other than Node.js which I usually write. This is the first .NET core app that I wrote that runs on Linux.

### What's Next?

1. Port this to another language, e.g. Python or Node.js or GoLang.

2. Use lambda functions for this.

3. Make this an extensible framework to be able to assume a role, perform some action to get data in other accounts and do roll-up reporting.