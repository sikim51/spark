(window.webpackJsonp=window.webpackJsonp||[]).push([[5],{963:function(e,t,a){"use strict";a.r(t),a.d(t,"_frontmatter",(function(){return r})),a.d(t,"default",(function(){return p}));a(11),a(6),a(5),a(3),a(7),a(4),a(8),a(1);var n=a(78),o=a(959);function l(){return(l=Object.assign||function(e){for(var t=1;t<arguments.length;t++){var a=arguments[t];for(var n in a)Object.prototype.hasOwnProperty.call(a,n)&&(e[n]=a[n])}return e}).apply(this,arguments)}var r={};void 0!==r&&r&&r===Object(r)&&Object.isExtensible(r)&&Object.defineProperty(r,"__filemeta",{configurable:!0,value:{name:"_frontmatter",filename:"src/pages/Deployment/Deploy_spark.md"}});var i={_frontmatter:r},b=o.a;function p(e){var t=e.components,a=function(e,t){if(null==e)return{};var a,n,o={},l=Object.keys(e);for(n=0;n<l.length;n++)a=l[n],t.indexOf(a)>=0||(o[a]=e[a]);return o}(e,["components"]);return Object(n.b)(b,l({},i,a,{components:t,mdxType:"MDXLayout"}),Object(n.b)("p",null,"This page describes how to deploy Spark from the source code onto a Windows machine, either virtual or physical. The instructions are based on Windows Server 2012."),Object(n.b)("h1",{id:"install-prerequisites"},"Install prerequisites"),Object(n.b)("p",null,"Install the following prerequisites."),Object(n.b)("p",null,"Please make sure to install IIS first, and ASP.Net 4.6 later. IIS comes with ASP.Net 4.5, and if you do 4.6 first, the IIS installation will break 4.6."),Object(n.b)("h2",{id:"install-internet-information-server-iis"},"Install Internet Information Server (IIS)"),Object(n.b)("p",null,"If you do not already have a recent version of IIS (7 or 8), install it."),Object(n.b)("ol",null,Object(n.b)("li",{parentName:"ol"},"Open Server Manager > Dashboard"),Object(n.b)("li",{parentName:"ol"},"Click 'Add roled and features'"),Object(n.b)("li",{parentName:"ol"},"Select 'Web Server (IIS)'"),Object(n.b)("li",{parentName:"ol"},"Make sure to include Management Tools > Management Service\nYou need this if you want to enable ",Object(n.b)("a",l({parentName:"li"},{href:"http://www.iis.net/learn/publish/using-web-deploy/introduction-to-web-deploy"}),"Web Deploy")," for non-administrators.")),Object(n.b)("h2",{id:"install-web-deploy"},"Install Web Deploy"),Object(n.b)("ol",null,Object(n.b)("li",{parentName:"ol"},"Download the ",Object(n.b)("a",l({parentName:"li"},{href:"https://www.microsoft.com/web/downloads/platform.aspx"}),"Web Platform Installer")," and run it (wpilauncher.exe)."),Object(n.b)("li",{parentName:"ol"},"Select 'WebDeploy 3.6 for Hosting Servers'"),Object(n.b)("li",{parentName:"ol"},"Install")),Object(n.b)("p",null,"Read ",Object(n.b)("a",l({parentName:"p"},{href:"https://www.iis.net/learn/install/installing-publishing-technologies/installing-and-configuring-web-deploy-on-iis-80-or-later"}),"more info")," about Web Deploy, especially for configuring Web Deploy for non-administrators."),Object(n.b)("h2",{id:"install-aspnet-46"},"Install ASP.NET 4.6"),Object(n.b)("p",null,"If you do not already have it installed, install ASP.NET 4.6 (version 4.5 is not enough)."),Object(n.b)("ol",null,Object(n.b)("li",{parentName:"ol"},"Download it from ",Object(n.b)("a",l({parentName:"li"},{href:"http://www.microsoft.com/en-us/download/details.aspx?id=48130"}),"Microsoft"),"."),Object(n.b)("li",{parentName:"ol"},"Install it by running the just downloaded NDP46-KB3045560-Web.exe.")),Object(n.b)("h2",{id:"install-mongodb"},"Install MongoDB"),Object(n.b)("p",null,"Spark uses MongoDB for storage. Install it as a service. These instructions are based on the ",Object(n.b)("a",l({parentName:"p"},{href:"https://docs.mongodb.com/v3.0/tutorial/install-mongodb-on-windows/"}),"MongoDB documentation"),"."),Object(n.b)("ol",null,Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Download ",Object(n.b)("a",l({parentName:"p"},{href:"https://www.mongodb.com/download-center?jmp=nav#community"}),"MongoDB"),", choose the Community Edition, 'Windows Server 2008 R2 and later, with SSL support'.")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Run the installer.\nBy default it will be installed in C:\\Program Files\\MongoDB\\Server")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Create directories for the database, log files and configuration of Mongo. For example:"),Object(n.b)("ul",{parentName:"li"},Object(n.b)("li",{parentName:"ul"},"C:\\Spark\\MongoDb\\Data"),Object(n.b)("li",{parentName:"ul"},"C:\\Spark\\MongoDb\\Log"),Object(n.b)("li",{parentName:"ul"},"C:\\Spark\\MongoDb\\Config"))),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Create a configuration file for MongoDb in the Config directory, name it SparkMongoDB.cfg and add the following to it (adjust to your previously created directories). Please note the indentation: it is relevant (MongoDB uses YAML for this configuration file)."),Object(n.b)("pre",{parentName:"li"},Object(n.b)("code",l({parentName:"pre"},{className:"language-yaml"}),"    systemLog:\n        destination: file\n        path: c:\\Spark\\MongoDB\\Log\\mongod.log\n    storage:\n        dbPath: c:\\Spark\\MongoDB\\Data\n"))),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Run the command below in a Command Window, as Administrator. It will register MongoDB as a Windows Service."),Object(n.b)("pre",{parentName:"li"},Object(n.b)("code",l({parentName:"pre"},{className:"language-dosbatch"}),'    c:\\Program Files\\MongoDB\\Server\\3.2\\bin>mongod.exe --config "C:\\Spark\\MongoDB\\Config\\SparkMongoDB.cfg" --install\n')))),Object(n.b)("h1",{id:"opening-ports"},"Opening Ports"),Object(n.b)("p",null,"If you want to deploy Spark with Web Deploy from within Visual Studio to this machine, you will have to open the relevant ports for it for inbound traffic:"),Object(n.b)("ul",null,Object(n.b)("li",{parentName:"ul"},"http (80)"),Object(n.b)("li",{parentName:"ul"},"https (443)"),Object(n.b)("li",{parentName:"ul"},"WebDeploy (8172)")),Object(n.b)("p",null,"Depending on your network configuration you may need to open 8172 locally for outbound traffic."),Object(n.b)("p",null,"If you want to manage the MongoDB database remotely, you also have to open:"),Object(n.b)("ul",null,Object(n.b)("li",{parentName:"ul"},"27017")),Object(n.b)("p",null,"Please take precautions when you do this (refer to MongoDB documentation for further information)."),Object(n.b)("ul",null,Object(n.b)("li",{parentName:"ul"},"Configure MongoDB security"),Object(n.b)("li",{parentName:"ul"},"Start MongoDB in secure mode")),Object(n.b)("h1",{id:"deploy-spark"},"Deploy Spark"),Object(n.b)("h2",{id:"by-web-deploy"},"By Web Deploy"),Object(n.b)("ol",null,Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Open Spark.sln in Visual Studio")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Choose Build | Publish Spark"),Object(n.b)("ol",{parentName:"li"},Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"On the Profile tab, create a new profile for this Virtual Machine:")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Publish target 'Custom' > choose a name (something that refers to this machine)")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Connection"),Object(n.b)("ol",{parentName:"li"},Object(n.b)("li",{parentName:"ol"},"Publish method: Web Deploy"),Object(n.b)("li",{parentName:"ol"},"Server: the ip address of your Virtual Machine"),Object(n.b)("li",{parentName:"ol"},"Site name: Default Web Site/spark"),Object(n.b)("li",{parentName:"ol"},"User name: login name for your VM"),Object(n.b)("li",{parentName:"ol"},"Password: matching the User name"),Object(n.b)("li",{parentName:"ol"},"Destination URL: can be left blank"),Object(n.b)("li",{parentName:"ol"},"Validate Connection"),Object(n.b)("li",{parentName:"ol"},"If the connection does not validate, check with the system administrator if the firewall lets you through."))),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Settings"),Object(n.b)("ol",{parentName:"li"},Object(n.b)("li",{parentName:"ol"},"Configuration: Release"))),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Publish"))))),Object(n.b)("h2",{id:"by-web-deploy-package"},"By Web Deploy Package"),Object(n.b)("ol",null,Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Open Spark.sln in Visual Studio")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Choose Build | Publish Spark"),Object(n.b)("ol",{parentName:"li"},Object(n.b)("li",{parentName:"ol"},"On the Profile tab, create a new profile."),Object(n.b)("li",{parentName:"ol"},"Publish target 'Custom' > choose a name (it may or may not refer to this specific machine, since you can deploy the package to other machines as well)."),Object(n.b)("li",{parentName:"ol"},"Connection",Object(n.b)("ol",{parentName:"li"},Object(n.b)("li",{parentName:"ol"},"Publish method: Web Deploy Package"),Object(n.b)("li",{parentName:"ol"},"Package location: choose as suitable directory on your disk."),Object(n.b)("li",{parentName:"ol"},"Site name: Default Web Site/spark"))),Object(n.b)("li",{parentName:"ol"},"Settings",Object(n.b)("ol",{parentName:"li"},Object(n.b)("li",{parentName:"ol"},"Configuration: Release"))),Object(n.b)("li",{parentName:"ol"},"Publish"))),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Connect Remote Desktop Manager to your VM, and select the option to make your local disks available to the VM.")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"In the VM, open Windows Explorer and copy the deployment package to a local directory on the VM.")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Read (from the deployment directory) the Spark.deploy-readme.txt.")),Object(n.b)("li",{parentName:"ol"},Object(n.b)("p",{parentName:"li"},"Start a Command Window as Administrator and run (from the deployment directory) "),Object(n.b)("p",{parentName:"li"},"Spark.deploy.cmd /Y"))),Object(n.b)("h1",{id:"check-spark"},"Check Spark"),Object(n.b)("p",null,"Open a browser on the server and try ",Object(n.b)("a",l({parentName:"p"},{href:"http://localhost/spark"}),"http://localhost/spark")),Object(n.b)("p",null,"If that works, try the same from your own machine, with ",Object(n.b)("a",l({parentName:"p"},{href:"http://%5C"}),"http://\\"),"<ip-adress-of-spark-machine",">","/spark"),Object(n.b)("h1",{id:"faq"},"FAQ"),Object(n.b)("h2",{id:"bad-module"},"Bad Module"),Object(n.b)("p",null,"When I first try to open Spark in the browser I get this error:"),Object(n.b)("pre",null,Object(n.b)("code",l({parentName:"pre"},{}),'Handler "ExtensionlessUrlHandler-Integrated-4.0" has a bad module "ManagedPipelineHandler" in its module list\n')),Object(n.b)("p",null,"Something must have gone wrong with installing ASP.Net in IIS. I (Christiaan) had this and solved it by running in a Command Window (as Administrator):"),Object(n.b)("pre",null,Object(n.b)("code",l({parentName:"pre"},{}),"dism.exe /Online /Enable-Feature /all /FeatureName:IIS-ASPNET45\n")),Object(n.b)("p",null,"Source of this fix: ",Object(n.b)("a",l({parentName:"p"},{href:"https://www.microsoft.com/en-us/download/details.aspx?id=44989"}),"https://www.microsoft.com/en-us/download/details.aspx?id=44989")))}p&&p===Object(p)&&Object.isExtensible(p)&&Object.defineProperty(p,"__filemeta",{configurable:!0,value:{name:"MDXContent",filename:"src/pages/Deployment/Deploy_spark.md"}}),p.isMDXComponent=!0}}]);
//# sourceMappingURL=component---src-pages-deployment-deploy-spark-md-40a07c063da7b1ff8244.js.map