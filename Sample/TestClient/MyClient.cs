﻿using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestClient
{
    /// <summary>
    /// <para>功能：</para>
    /// <para>作者：hz0704027 </para>
    /// <para>日期：2015/10/12 11:24:25 </para>
    /// <para>备注：本代码版权归慧择网所有，严禁外传 </para>
    /// </summary>
	public class MyClient
    {
        static IConnection _connection;
        static IModel _channel;
        static SimpleRpcClient _client;

        static IConnection GetConnection
        {
            get
            {
                if(null== _connection)
                {
                    ConnectionFactory factory = new ConnectionFactory
                    {
                        HostName = "localhost",
                        UserName = "guest",
                        Password = "guest",
                        Port = 5672,
                        VirtualHost = "/"
                    };
                    _connection = factory.CreateConnection();
                }

                return _connection;
            }
        }

        static SimpleRpcClient GetClient
        {
            get
            {
                if (null == _client)
                {
                    if(null == _channel)
                    {
                        _channel = GetConnection.CreateModel();
                    }

                    _client = new SimpleRpcClient(_channel, "test.request.reply.queue")
                    {
                        TimeoutMilliseconds = 4000
                    };

                    _client.TimedOut += TimedOutHandler;
                    _client.Disconnected += DisconnectedHandler;
                }

                return _client;
            }
        }

        private static void DisconnectedHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void TimedOutHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Timeout");
        }

        public void Call()
        {
            //IBasicProperties replyProp;
            //IBasicProperties prop = _channel.CreateBasicProperties();
            GetClient.Call(Encoding.UTF8.GetBytes("test"));
        }
    }
}