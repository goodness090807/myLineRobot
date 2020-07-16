using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using isRock.LineBot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using myLineRobot.Models;

namespace isRock.Template
{
    public class LineWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        private readonly string _mongodbConnectString;
        private readonly string _ChannelAccessToken;

        public LineWebHookController(IConfiguration configuration)
        {
            _mongodbConnectString = configuration.GetValue<string>("MONGODB_URI");
            _ChannelAccessToken = configuration.GetValue<string>("ChannelAccessToken");
        }

        [Route("api/LineBotWebHook")]
        [HttpPost]
        public IActionResult POST()
        {
            var AdminUserId = "_UserID_";
            var client = new MongoClient(_mongodbConnectString);

            try
            {
                //設定ChannelAccessToken
                this.ChannelAccessToken = _ChannelAccessToken;
                //取得Line Event
                var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                var responseMsg = "";
                //準備回覆訊息
                if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
                {

                    var database = client.GetDatabase("heroku_gdl1gq89");
                    var collection = database.GetCollection<UserMessage>("UserMessage");

                    if (LineEvent.message.text.StartsWith("!get "))
                    {
                        List<UserMessage> document = collection.AsQueryable()
                                .Where(x => x.UserName == LineEvent.message.text.Replace("!get ", ""))
                                .OrderByDescending(y => y.TextTime)
                                .Take(5)
                                .ToList();
                        StringBuilder sbMsg = new StringBuilder();
                        foreach (UserMessage item in document)
                        {
                            sbMsg.AppendLine($"{item.TextTime}  {item.UserName}說了：{item.Content}");
                        }

                        responseMsg = sbMsg.ToString();


                        //回覆訊息
                        this.ReplyMessage(LineEvent.replyToken, responseMsg);
                        //response OK
                        return Ok();
                    }
                    else
                    {
                        LineUserInfo requestUser = this.GetUserInfo(LineEvent.source.userId);

                        UserMessage userMessage = new UserMessage()
                        {
                            UserName = requestUser.displayName,
                            Content = LineEvent.message.text,
                            TextTime = DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss")
                        };

                        collection.InsertOne(userMessage);
                    }


                }

                // else if (LineEvent.type.ToLower() == "message")
                //     responseMsg = $"收到 event : {LineEvent.type} type: {LineEvent.message.type} ";
                // else
                //     responseMsg = $"收到 event : {LineEvent.type} ";
                //回覆訊息
                this.ReplyMessage(LineEvent.replyToken, "收到訊息囉");
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                //回覆訊息
                // this.PushMessage(AdminUserId, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }
    }
}