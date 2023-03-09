# Descriptions
**GoogleDriveUploader** — is a **Windows Service** that syncs your local folder with your **Google Drive**. 
That app will copy folder and all files inside, track any changes (files creation, modification, deletion) and publish them into your **Google Drive** account.

# Used Technologies
- **Asp.Net Core 6**
- **Google Drive api v3**
- **LiteDb**
- **NUnit, Moq**

# Application architecture
![image](https://i.ibb.co/kHzpjCy/3-2.jpg)

# Workflow diagram
![image](https://i.ibb.co/5FyjwJP/1-1.jpg)

# How to install?
There is archived builded **GoogleDriveUploader** in **Published App** folder in repository. 
![image](https://i.ibb.co/XDr2rbN/image-2023-02-02-14-24-50.png)
You must unzip it to any folder you want.
Than put folder path you want to be copy to **Google Drive** in appsettings.json.

![image](https://i.ibb.co/qRdVG9P/Screenshot-3.png)

**GoogleDriveUploader** requires authorization via redirect in browser. So first of all you need to open **GoogleDriveUploader.Worker.exe** which will open your browser to authorize
your Google Drive account and create windows service. 
After authorization opened console will automaticily close. 

![image](https://i.ibb.co/jG71F92/Screenshot-2.png)


And that's all!, now **GoogleDriveUploader** as windows service will copy your folder and all its changes to your **Google Drive**.

![image](https://i.ibb.co/nPFzYNk/photo-2023-02-03-08-55-50.jpg)

You can see **GoogleDriveUploader** Windows Service in **Services**. It'll always open when Windows start.

![image](https://i.ibb.co/d0ty6Nf/Screenshot-1.png)


