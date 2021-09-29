# File replication over FTP between multiple servers
This class offers great functionality to implement file replication between local and remote servers over FTP. File Replication offers 3 main functionalities independent of operating system environments (e.g. from Windows Server to Lunix FTP Server, etc.):

- Put Stream: Upload file from user browser to web server as PostedStream then pass the stream to remote FTP server. No temp storage on the web server is required. There is also an option to put locally stored file on a web server onto remote FTP server.

- Get Stream: Grab a file from remote FTP server and pass it as stream to the web server, then flush it as response to the user browser. No temp storage on the web server is required

- Delete File: Delete a file on remote FTP server and return the status of the operation

To begin with, you need to set the FTP connection settings:

FTP_SERV: Full FTP Uri
FTP_USER: FTP username
FTP_USER_PASSWD: FTP password

To start replicating:

        FileReplication replicater = new FileReplication();
        int res = replicater.OnFtpPutStream(fbUploader.PostedFile, fbUploader.FileName);
        if (res==200)
            Response.Write("ok");
        else
            Response.Write("not uploaded");
        res = replicater.OnFtpGetStream(filename);
        
# Status codes
200: OK
300: File Not Allowed
500: FTP Server is offline or the file does not exist

# Use case
After finishing storing the file on local server (e.g. fbUploader.SaveAs()), you can call OnFtpPutFile() to replicate the file on some remote FTP server, thus you are performing inline backup for files while performing file uploading.

Another usage is convert your web server to business logic server only whilst relying on external server for file storage only. This can be done by calling OnFtpPutStream, where you don't need to store anything on your main web server.

Another usage is to perform handy file caching during colocation or load balancing by keeping frequently accessed files on the main web browser, whilst removing them on expiry and pull fresh copies. In such use case, I strongly recommend to check Redis Server.

# Credit
This class is not dependent on any external libraries. It was written by me to enable IT operations expansion in University of Babylon. For contacting me:

Dr. Hassan H. Alrehamy
h@uobabylon.edu.iq
