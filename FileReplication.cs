using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

/// <summary>
/// Main library for managing remote mirroring and backup
/// </summary>
public class FileReplication
{
    #region FTP Settings
    static readonly string FTP_SERV = "";
    static readonly string FTP_USER = "";
    static readonly string FTP_USER_PASSWD = "";
    /// <summary>
    /// List of allowed file extensions for replication on remote FTP server
    /// </summary>
    public static readonly List<string> FTP_MIMEALLOWED = new List<string>() { ".pdf", ".doc", ".docx", ".csv", ".jpg", ".jpeg", ".gif", ".png", ".rar", ".pptx" };
    /// <summary>
    /// Hash table of translation between allowed file extensions and their translation
    /// </summary>
    public static readonly Dictionary<string, string> FTP_MIMETRANS = new Dictionary<string, string>() 
                                        { { ".pdf", "pdf" }, 
                                        { ".doc", "msword" }, 
                                        { ".csv", "csv" }, 
                                        { ".jpg", "jpeg" },
                                        { ".jpeg", "jpeg" },
                                        { ".gif", "gif" },
                                        { ".png", "png" },
                                        {".rar","vnd.rar"},
                                        { ".docx", "vnd.openxmlformats-officedocument.wordprocessingml.document" }, 
                                        { ".pptx", "vapplication/vnd.openxmlformats-officedocument.presentationml.presentation" } };
    #endregion

    #region FTP Services
    /// <summary>
    /// Sends the specified file stored locally to remote FTP server. localserver is local physical path. Return status code.
    /// </summary>
    public int OnFtpPutFile(string localserver,string filename)
    {
        try
        {
            if (FTP_MIMEALLOWED.Contains(Path.GetExtension(filename).ToLower()))
            {
                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(FTP_USER, FTP_USER_PASSWD);
                    client.UploadFile(FTP_SERV + filename, WebRequestMethods.Ftp.UploadFile, localserver + filename);
                }
                return 200;
            }
            else
                return 300;
        }
        catch (System.Net.WebException)
        {
            return 500;
        }
    }
    /// <summary>
    /// Sends a posted file stream (e.g. from FileUpload) with the specified name to remote FTP server. No local storage required. Returns status code.
    /// </summary>
    public int OnFtpPutStream(HttpPostedFile FilePost, string filename)
    {
        try
        {
            if (FTP_MIMEALLOWED.Contains(Path.GetExtension(filename).ToLower()))
            {
                byte[] fileBytes = null;

                using (BinaryReader binReader = new BinaryReader(FilePost.InputStream))
                {
                    fileBytes = binReader.ReadBytes(FilePost.ContentLength);
                }

                FtpWebRequest FTP_request = (FtpWebRequest)WebRequest.Create(FTP_SERV + filename);

                FTP_request.Method = WebRequestMethods.Ftp.UploadFile;
                FTP_request.Credentials = new NetworkCredential(FTP_USER, FTP_USER_PASSWD);
                FTP_request.ServicePoint.ConnectionLimit = fileBytes.Length;
                FTP_request.ContentLength = fileBytes.Length;
                FTP_request.EnableSsl = false;
                FTP_request.KeepAlive = false;

                using (Stream requestStream = FTP_request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Close();
                }

                using (FtpWebResponse response = (FtpWebResponse)FTP_request.GetResponse()) { };

                return 200;
            }
            else
                return 300;
        }
        catch (System.Net.WebException)
        {
            return 500;
        }
    }
    /// <summary>
    /// Downloads a stream of the specified file from remote FTP server and flushes it directly into the user's browser. No local storage required. Returns status code.
    /// </summary>
    public int OnFtpGetStream(string filename)
    {
        try
        {
            if (FTP_MIMEALLOWED.Contains(Path.GetExtension(filename).ToLower()))
            {
                FtpWebRequest FTP_request = (FtpWebRequest)WebRequest.Create(FTP_SERV + filename);
                FTP_request.Method = WebRequestMethods.Ftp.DownloadFile;
                FTP_request.Credentials = new NetworkCredential(FTP_USER, FTP_USER_PASSWD);
                FTP_request.EnableSsl = false;
                FTP_request.KeepAlive = false;

                using (Stream stream = ((FtpWebResponse)FTP_request.GetResponse()).GetResponseStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        if (memoryStream.CanRead)
                        {
                            HttpContext.Current.Response.Clear();
                            HttpContext.Current.Response.ContentType = "application/" + FTP_MIMETRANS[Path.GetExtension(filename).ToLower()];
                            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);
                            stream.CopyTo(memoryStream);
                            HttpContext.Current.Response.BinaryWrite(memoryStream.ToArray());
                            HttpContext.Current.Response.Flush();
                            HttpContext.Current.Response.Close();
                            HttpContext.Current.Response.End();
                        }
                    }
                };

                return 200;
            }
            else
                return 300;

        }
        catch (System.Net.WebException)
        {
            return 500;
        }
    }
    /// <summary>
    // Deletes the specified file from remote FTP server. Returns status code.
    /// </summary>
    public int OnFtpDelete(string filename)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTP_SERV + filename);
            request.Credentials = new NetworkCredential(FTP_USER, FTP_USER_PASSWD);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) { };
            return 200;
        }
        catch (System.Net.WebException)
        {
            return 500;
        }
    }
    #endregion
}