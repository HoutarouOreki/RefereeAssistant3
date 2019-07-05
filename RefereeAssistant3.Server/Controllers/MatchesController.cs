using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using RefereeAssistant3.Main.Online.APIModels;
using RefereeAssistant3.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace RefereeAssistant3.Server.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class MatchesController : Controller
    {
        private readonly MatchService matchService;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MatchesController(MatchService _matchService) => matchService = _matchService;

        [HttpGet("find/{matchCode}")]
        public ActionResult<List<APIMatch>> Find(string matchCode) => matchService.Find(matchCode);

        [HttpPost("new")]
        public ActionResult<APIMatch> CreateNew()
        {
            var text = new StreamReader(HttpContext.Request.Body).ReadToEnd();
            APIMatch match;

            try
            {
                match = JsonConvert.DeserializeObject<APIMatch>(text);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to deserialize new match:\n{e.Message}\n{text}");
                return BadRequest();
            }

            return matchService.Add(match);
        }

        [HttpPut("update/{matchId}")]
        public ActionResult<APIMatch> Update(int matchId)
        {
            var text = new StreamReader(HttpContext.Request.Body).ReadToEnd();
            APIMatch match;

            try
            {
                match = JsonConvert.DeserializeObject<APIMatch>(text);
            }
            catch (Exception)
            {
                logger.Error($"Failed to deserialize new match:\n{text}");
                return BadRequest();
            }
            var updatedMatch = matchService.Update(matchId, match);
            if (updatedMatch == null)
            {
                logger.Warn($"Failed to update match {matchId}, because it does not exist\n{text}");
                return NotFound();
            }
            return updatedMatch;
        }

        [HttpGet("{matchId}")]
        public ActionResult<APIMatch> Get(int matchId) => matchService.Get(matchId);
    }
}
