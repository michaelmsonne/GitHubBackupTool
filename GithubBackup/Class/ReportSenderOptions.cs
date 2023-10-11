﻿using System;
using System.Net.Mail;
using static GithubBackup.Class.FileLogger;

namespace GithubBackup.Class
{
    internal class ReportSenderOptions
    {
        public static MailPriority ParseEmailPriority(string priorityString)
        {
            switch (priorityString.ToLower())
            {
                case "low":
                    Message("Email report priority arguments is set to: Low", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Email report priority arguments is set to: Low");
                    return MailPriority.Low;
                case "high":
                    Message("Email report priority arguments is set to: High", EventType.Information, 1000);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Email report priority arguments is set to: High");
                    return MailPriority.High;
                default:
                    Message("Invalid email priority argument. Defaulting to normal Mail Priority.", EventType.Warning, 1000);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Invalid email priority argument. Defaulting to normal Mail Priority.");
                    return MailPriority.Normal;
            }
        }
    }
}