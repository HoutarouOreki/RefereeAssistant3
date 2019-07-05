using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RefereeAssistant3.Main.Online.APIModels;
using System.Collections.Generic;

namespace RefereeAssistant3.Server.Services
{
    public class MatchService
    {
        private readonly IMongoCollection<APIMatch> matches;

        public MatchService()
        {
            var client = new MongoClient(Settings.ConnectionString);
            var database = client.GetDatabase("referee-assistant-3");
            matches = database.GetCollection<APIMatch>("matches");
        }

        public List<APIMatch> Find(string matchCode) => matches.Find(m => m.Code == matchCode).ToList();
        public APIMatch Get(int matchId) => matches.Find(m => m.Id == matchId).FirstOrDefault();

        public ActionResult<APIMatch> Add(APIMatch match)
        {
            var highestId = matches.Find(_ => true).SortByDescending(m => m.Id).Limit(1).FirstOrDefault()?.Id ?? 0;
            match.Id = highestId + 1;
            matches.InsertOne(match);
            return match;
        }

        public ActionResult<APIMatch> Update(int matchId, APIMatch match)
        {
            var find = Builders<APIMatch>.Filter.Eq(m => m.Id, matchId);
            return matches.FindOneAndReplace(find, match, new FindOneAndReplaceOptions<APIMatch>
            {
                ReturnDocument = ReturnDocument.After
            });
        }
    }
}
