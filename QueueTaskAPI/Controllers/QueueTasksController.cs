using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueTaskAPI.Models;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using Microsoft.AspNetCore.Cors;

namespace QueueTaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  // [EnableCors("AllowOrigin")]
    public class QueueTasksController : ControllerBase
    {
        private readonly QueueTaskDBContexts _context;
        private ConnectionFactory factory;
        public QueueTasksController(QueueTaskDBContexts context)
        {
            _context = context;
        }

        private void InitRabbitMQ()
        {
            factory = new ConnectionFactory()
            {
                HostName = "localhost",
                //localhost
                //host.docker.internal
                Port = 31672
                //  HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                // Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };
        }

        // GET: api/QueueTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QueueTask>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/QueueTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QueueTask>> GetQueueTask(int id)
        {
            var queueTask = await _context.TodoItems.FindAsync(id);

            if (queueTask == null)
            {
                return NotFound();
            }

            return queueTask;
        }

        // PUT: api/QueueTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQueueTask(int id, QueueTask queueTask)
        {
            if (id != queueTask.TaskId)
            {
                return BadRequest();
            }

            _context.Entry(queueTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QueueTaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/QueueTasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<QueueTask>> PostQueueTask(QueueTask queueTask)
        {
            _context.TodoItems.Add(queueTask);
            await _context.SaveChangesAsync();
            postRabitQueue(queueTask);
            return CreatedAtAction("GetQueueTask", new { id = queueTask.TaskId }, queueTask);
        }

        // DELETE: api/QueueTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQueueTask(int id)
        {
            var queueTask = await _context.TodoItems.FindAsync(id);
            if (queueTask == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(queueTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QueueTaskExists(int id)
        {
            return _context.TodoItems.Any(e => e.TaskId == id);
        }

        private void postRabitQueue(QueueTask objUI)
        {

            string output = JsonConvert.SerializeObject(objUI);
            //dynamic objParsedata = Newtonsoft.Json.Linq.JObject.Parse(objUI);


            //string sdata = ((Newtonsoft.Json.Linq.JContainer)objParsedata).ToString();

            if (factory == null)
            {
                factory = new ConnectionFactory()
                {
                    HostName = "host.docker.internal", // localhost
                    //  docker: host.docker.internal
                    Port = 31672
                    //  HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                    // Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
                };
            }

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {


                var body = Encoding.UTF8.GetBytes(output);

                channel.BasicPublish(exchange: "",
                                     routingKey: "tasks",
                                     basicProperties: null,
                                     body: body);
            }

        }
    }
}
