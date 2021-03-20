using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using OrbitalShell_WebAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrbitalShell_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShellController : ControllerBase
    {
        // GET: api/<ShellController>
        [HttpGet]
        public ShellExecResult Get()
        {
            return ShellExecResult.EmptyShellExecResult;
        }

        // GET api/<ShellController>/5
        /*[HttpGet("{id}")]
        public ShellExecResult Get(int id)
        {
            return ShellExecResult.EmptyShellExecResult;
        }*/

        // POST api/<ShellController>
        [HttpPost]
        public ShellExecResult Post([FromBody] string commandLine)
        {
            return ShellExecResult.EmptyShellExecResult;
        }

        // PUT api/<ShellController>/5
        /*[HttpPut("{id}")]
        public ShellExecResult Put(int id, [FromBody] string value)
        {
            return ShellExecResult.EmptyShellExecResult;
        }*/

        // DELETE api/<ShellController>/5
        /*[HttpDelete("{id}")]
        public ShellExecResult Delete(int id)
        {
            return ShellExecResult.EmptyShellExecResult;
        }*/
    }
}
