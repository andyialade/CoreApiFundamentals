using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/v{version:apiVersion}/camps")]
    [ApiVersion("2.0")]
    [ApiController]
    public class Camps2Controller : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        public Camps2Controller(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;           
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCampsCollection(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);
                var resultWithCount = new { Count = results.Length, Items = _mapper.Map<CampModel[]>(results) };
                return Ok(resultWithCount);

                //return _mapper.Map<CampModel[]>(results);

                //Elaborate
                //CampModel[] models = _mapper.Map<CampModel[]>(results);
                //return Ok(models);
            }
            catch (Exception)
            {
                //return BadRequest("Database Failure");
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }
        }


        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker, true);
                if (result == null)
                    return NotFound();

                return _mapper.Map<CampModel>(result);
                //return result != null ? _mapper.Map<CampModel>(result) : NotFound();
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any())
                    return NotFound();

                return _mapper.Map<CampModel[]>(results);

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                //Not working in .net core 3.1
                //var location = _linkGenerator.GetPathByAction(HttpContext, "Get", "Camps", new { moniker = model.Moniker });
                //if (string.IsNullOrWhiteSpace(location))
                //{
                //    return BadRequest("Could not use current moniker");
                //}

                //Validate Model
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Model");
                }

                var entity = _mapper.Map<Camp>(model);
                _repository.Add(entity);

                if(await _repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{entity.Moniker}", _mapper.Map<CampModel>(entity));
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }

            return BadRequest("Database Failure");
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null)
                    return NotFound($"Could not find camp with moniker of {model.Moniker}");

                _mapper.Map(model, oldCamp);
                if (await _repository.SaveChangesAsync())
                {
                    return  _mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }

            return BadRequest("Database Failure");
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null)
                    return NotFound($"Could not find camp with moniker of {moniker}");

                _repository.Delete(oldCamp);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); //Used for specific status codes
            }

            return BadRequest("Database Failure");
        }
    }
}
