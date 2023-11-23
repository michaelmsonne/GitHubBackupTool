using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class ReportSender
    {
        public static void SendEmail(string serverAddress, int serverPort, string emailFrom, string emailTo, string emailStatusMessage,
            List<string> repoCountElements, List<string> repoItemsCountElements, int repoCount, int repoItemsCount, string outDir, TimeSpan elapsedTime, int errors,
            int totalBackupsIsDeleted, int daysToKeep, string repoCountStatusText, string repoItemsCountStatusText,
            string totalBackupsIsDeletedStatusText, bool useSimpleMailReportLayout, string isDaysToKeepNotDefaultStatusText, string startTime, string endTime)
        {
            var serverPortStr = serverPort;
            string mailBody;
            //if (mailBody == null) throw new ArgumentNullException(nameof(mailBody));

            //Parse data to list from list of repo.name
            var listrepocountelements = "<h3>List of Git repositories in GitHub (the API key give access to (showing only main branch here)):</h3>∘ " + string.Join("<br>∘ ", repoCountElements);
            var listitemscountelements = "<h3>List of Git repositories in GitHub a backup is performed of (based on arguments for backup type):</h3>∘ " + string.Join("<br>∘ ", repoItemsCountElements);
            
            // Get email status text from job status
            if (Globals._isBackupOk)
            {
                // If unzipped or not
                emailStatusMessage = "Success";
            }
            else
            {
                emailStatusMessage = "Failed!";
            }

            // Text if no Git projects to backup
            if (Globals._noProjectsToBackup)
            {
                emailStatusMessage = "No projects to backup!";
            }

            // It error count is over 0 add warning in email subject
            if (errors > 0)
            {
                emailStatusMessage += " - but with warning(s)";
            }

            // Old backup(s) deleted in backup folder:
            if (Globals._totalBackupsIsDeleted != 0)
            {
                totalBackupsIsDeletedStatusText = "Good - deleted " + Globals._totalBackupsIsDeleted + " old backup(s) from backup folder";

                // Log
                Message($"Old backup(s) deleted in backup folder: status: " + totalBackupsIsDeletedStatusText, EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Old backup(s) deleted in backup folder: status: " + totalBackupsIsDeletedStatusText);
                Console.ResetColor();
            }
            else
            {
                if (Globals._isBackupOk)
                {
                    totalBackupsIsDeletedStatusText = "Good - no old backup(s) to delete from backup folder";

                    // Log
                    Message($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText, EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText);
                    Console.ResetColor();
                }
                else
                {
                    totalBackupsIsDeletedStatusText = "Warning!";

                    // Log
                    Message($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText, EventType.Warning, 1001);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Old backup(s) deleted in backup folder status: " + totalBackupsIsDeletedStatusText);
                    Console.ResetColor();
                }
            }
            // TODO - CLEANUP LATER
            // Globals._totalBackupsIsDeletedStatusText = totalBackupsIsDeletedStatusText;
            
            // If args is set to old mail report layout
            if (useSimpleMailReportLayout)
            {
                // Make email body data
                mailBody =
                    $"<hr><h2>Your {Globals._appName} of organization/for user '{Globals._name}' is: {emailStatusMessage}</h2><hr><p><h3>Details:</h3><p>" +
                    $"<p>Processed Git repositories in GitHub API key gives access to (total): <b>{repoCount}</b><br>" +
                    $"Processed Git repos in repositories a backup is made of from GitHub: <b>{Globals._repoBackupPerformedCount}</b><p>" +
                    $"See the attached logfile for the backup(s) today: <b>{Globals._appName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + ".log</b>.<p>" +
                    $"Total Run Time is: \"{elapsedTime}\"<br>" +
                    $"Backup start Time: \"{startTime}\"<br>" +
                    $"Backup end Time: \"{endTime}\"<br>" +
                    //"<h3>Download cleanup (if specified):</h3><p>" +
                    //$"Leftovers for original downloaded <b>.zip</b> files in backup folder (error(s) when try to delete): <b>{letOverZipFiles}</b><br>" +
                    //$"Leftovers for original downloaded <b>.json</b> files in backup folder (error(s) when try to delete): <b>{letOverJsonFiles}</b><p>" +
                    $"<h3>Backup location:</h3><p>Backed up in folder: <b>\"{outDir}\"</b> on host/server: <b>{Environment.MachineName}</b><br>" +
                    $"Old backups set to keep in backup folder (days): <b>{daysToKeep}</b><br>" +
                    $"Old backups deleted in backup folder: <b>{totalBackupsIsDeleted}</b><br>" +
                    listrepocountelements + "<br>" +
                    listitemscountelements + "</p><br><hr>" +
                    $"<h3>From Your {Globals._appName} tool!<o:p></o:p></h3>" + Globals._copyrightData + ", v." + Globals._vData;
            }
            else
            {
                // Make email body data
                mailBody =
                $"<hr/><h2>Your {Globals._appName} of organization/for user '{Globals._name}' is: {emailStatusMessage}</h2><hr />" +
                $"<br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\">" +
                $"<tbody><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\"><strong>Backup task(s):</strong></td>" +
                $"<td style=\"width: 10%; height: 18px;\"><strong>File(s):</strong></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed Git repositories in GitHub API key gives access to (total):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{repoCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{repoCountStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed Git repos in GitHub a backup is made of from GitHub:</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{Globals._repoBackupPerformedCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{repoItemsCountStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed branches for backup from Git repos (all branches):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{Globals._repoBackupPerformedBranchCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">XX</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 33%; height: 18px;\">Processed folders to backup from Git repos (total folders) (all branches):</td>" +
                $"<td style=\"width: 10%; height: 18px;\"><b>{Globals._backupFolderCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">XX</td></tr><tr>" +
                $"<td style=\"width: 33%;\">Processed files to backup from Git repos (total files) (all branches):</td>" +
                $"<td style=\"width: 10%;\"><b>{Globals._backupFileCount}</b></td>" +
                $"<td style=\"width: 33.3333%;\">XX</td></tr></tbody></table><br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\"><tbody><tr style=\"height: 18px;\">" +
                // $"<td style=\"width: 33.3333%; height: 18px;\">XXXXXXXXXXXXXXXXXX</td></tr></tbody></table><br><table style=\"border-collapse: collapse; width: 100%; height: 108px;\" border=\"1\"><tr>" +
                $"<td style=\"width: 21%; height: 18px;\"><strong>Backup:</strong></td>" +
                $"<td style=\"width: 22%; height: 18px;\"><strong>Info:</strong></td>" +
                $"<td style=\"width: 33%; height: 18px;\"><strong>Status:</strong></td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Backup folder:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><strong><b>\"{outDir}\"</b></b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">XX</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Backup server:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{Environment.MachineName}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">  </td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Old backup(s) set to keep in backup folder (days):</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{daysToKeep}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{isDaysToKeepNotDefaultStatusText}</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Number of current backups in backup folder:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{Globals._currentBackupsInBackupFolderCount}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">Multiple backups can be created at the same day, so there can be more backups then days set to keep</td></tr><tr style=\"height: 18px;\">" +
                $"<td style=\"width: 21%; height: 18px;\">Old backup(s) deleted in backup folder:</td>" +
                $"<td style=\"width: 22%; height: 18px;\"><b>{totalBackupsIsDeleted}</b></td>" +
                $"<td style=\"width: 33.3333%; height: 18px;\">{totalBackupsIsDeletedStatusText}</td></tr></table>" +
                $"<p>See the attached logfile for the backup(s) today: <b>'{Globals._appName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + ".log'</b>.<o:p></o:p></p>" +
                $"<p>Total Run Time is: \"{elapsedTime}\"<br>" +
                $"Backup start Time: \"{startTime}\"<br>" +
                $"Backup end Time: \"{endTime}\"</p><hr/>" +
                listrepocountelements + "<br>" +
                listitemscountelements + "</p><br><hr>" +
                $"<h3>From Your {Globals._appName} tool!<o:p></o:p></h3>" + Globals._copyrightData + ", v." + Globals._vData;
            }

            // Create mail
            var message = new MailMessage(emailFrom, emailTo);
            message.Subject = "[" + emailStatusMessage + $"] - {Globals._appName} status - (" + Globals._repoBackupPerformedCount +
                              " Git projects (" + Globals._repoBackupPerformedBranchCount + " branches) backed up), " + errors + " issues(s) - (backups to keep (days): " + daysToKeep +
                              ", backup(s) deleted: " + totalBackupsIsDeleted + ")";
            message.Body = mailBody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            // Set email priority level based on command-line argument
            message.Priority = Globals._emailPriority;
            message.DeliveryNotificationOptions = DeliveryNotificationOptions.None;
            message.BodyTransferEncoding = TransferEncoding.QuotedPrintable;

            // ReSharper disable once UnusedVariable
            using var client = new SmtpClient(serverAddress, serverPortStr);
            client.EnableSsl = true;
            client.UseDefaultCredentials = true;
            Message("Created email report and parsed data", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Created email report and parsed data");
            Console.ResetColor();

            // Get all the files in the log dir for today

            // Log
            Message("Finding logfile for today to attach in email report...", EventType.Information, 1000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Finding logfile for today to attach in email report...");
            Console.ResetColor();

            // Get filename to find
            var filePaths = Directory.GetFiles(Files.LogFilePath, $"{Globals._appName} Log " + DateTime.Today.ToString("dd-MM-yyyy") + "*.*");

            // Get the files that their extension are .log or .txt
            var files = filePaths.Where(filePath => Path.GetExtension(filePath).Contains(".log") || Path.GetExtension(filePath).Contains(".txt"));

            // Loop through the files enumeration and attach each file in the mail.
            foreach (var file in files)
            {
                Globals._fileAttachedIneMailReport = file;

                // Log
                Message("Found logfile for today", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Found logfile for today:");
                Console.ForegroundColor = ConsoleColor.White;
                //Console.ResetColor();

                // Full file name
                var fileName = Globals._fileAttachedIneMailReport;
                var fi = new FileInfo(fileName);

                // Get File Name
                var justFileName = fi.Name;
                Console.WriteLine("File name: '" + justFileName + "'");
                Message("File name: '" + justFileName + "'", EventType.Information, 1000);

                // Get file name with full path
                var fullFileName = fi.FullName;
                Console.WriteLine("Full file name: '" + fullFileName + "'");
                Message("Full file name: '" + fullFileName + "'", EventType.Information, 1000);

                // Get file extension
                var extn = fi.Extension;
                Console.WriteLine("File Extension: '" + extn + "'");
                Message("File Extension: '" + extn + "'", EventType.Information, 1000);

                // Get directory name
                var directoryName = fi.DirectoryName;
                Console.WriteLine("Directory name: '" + directoryName + "'");
                Message("Directory name: '" + directoryName + "'", EventType.Information, 1000);

                // File Exists ?
                var exists = fi.Exists;
                Console.WriteLine("File exists: " + exists);
                Message("File exists: " + exists, EventType.Information, 1000);
                if (fi.Exists)
                {
                    // Get file size
                    var size = fi.Length;
                    Console.WriteLine("File Size in Bytes: " + size);
                    Message("File Size in Bytes: " + size, EventType.Information, 1000);

                    // File ReadOnly ?
                    var isReadOnly = fi.IsReadOnly;
                    Console.WriteLine("Is ReadOnly: " + isReadOnly);
                    Message("Is ReadOnly: " + isReadOnly, EventType.Information, 1000);

                    // Creation, last access, and last write time
                    var creationTime = fi.CreationTime;
                    Console.WriteLine("Creation time: " + creationTime);
                    Message("Creation time: " + creationTime, EventType.Information, 1000);
                    var accessTime = fi.LastAccessTime;
                    Console.WriteLine("Last access time: " + accessTime);
                    Message("Last access time: " + accessTime, EventType.Information, 1000);
                    var updatedTime = fi.LastWriteTime;
                    Console.WriteLine("Last write time: " + updatedTime + "\n");
                    Message("Last write time: " + updatedTime, EventType.Information, 1000);
                }

                // TODO Do not add more to logfile here - file is locked!
                var attachment = new Attachment(file);

                // Attach file to email
                message.Attachments.Add(attachment);
            }

            //Try to send email status email
            try
            {
                // Send the email
                client.Send(message);

                // Release files for the email
                message.Dispose();
                // TODO logfile is not locked from here - you can add logs to logfile again from here!

                // Log
                Message("Email notification is send to '" + emailTo + "' at " + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + " with priority " + Globals._emailPriority + "!", EventType.Information, 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Email notification is send to '" + emailTo + "' at " + DateTime.Now.ToString("dd-MM-yyyy (HH-mm)") + " with priority " + Globals._emailPriority + "!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Log
                Message("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex, EventType.Error, 1001);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Sorry, we are unable to send email notification of your presence. Please try again! Error: " + ex);
                Console.ResetColor();
            }
        }
    }
}
