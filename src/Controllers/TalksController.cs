using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/talks")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }


        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> GetCollection(string moniker)
        {
            try
            {
                var result = await _repository.GetTalksByMonikerAsync(moniker);
                if (result == null)
                    return NotFound();

                return _mapper.Map<TalkModel[]>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetModel(string moniker, int id)
        {
            try
            {
                var result = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (result == null)
                    return NotFound();

                return _mapper.Map<TalkModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist");

                var entity = _mapper.Map<Talk>(model);
                entity.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");

                entity.Speaker = speaker;
                _repository.Add(entity);

                if (await _repository.SaveChangesAsync())
                {
                    //Not working in .net core 3.1
                    //var url = _linkGenerator.GetPathByAction(HttpContext, controller: "talks", action: "Get", values: new { moniker = moniker, id = entity.TalkId });

                    var url = $"/api/camps/{moniker}/talks/{entity.TalkId}";
                    return Created(url, _mapper.Map<TalkModel>(entity));
                }
                else
                {
                    return BadRequest("Failed to save new Talk");
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }

            
        }
    }
}
