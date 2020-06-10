using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Models;
using Orders;
using Prometheus;
namespace Orders.Controllers
{

    public class OrderThing
    {
        public int Making { get; set; }
        public string TIME  { get; set; }

    }

    [Route("api/[controller]")]
    [ApiController]
    public class OrderStatusController : ControllerBase
    {
        private readonly OrdersContext _context;

        public OrderStatusController(OrdersContext context)
        {
            _context = context;
        }

        // GET: api/OrderStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderStatus>>> GetOrderStatus()
        {
            return await _context.OrderStatus.ToListAsync();
        }

        [HttpPut("UpdateOrderTime/{id}")]

        public async Task<IActionResult> UpdateOrder(int id, OrderStatus orderStatus)
        {
            if (id != orderStatus.Id)
            {
                return BadRequest();
            }
            if (!OrderStatusExists(id))
            {
                return NotFound();
            }
            var orderStatus2 = await _context.OrderStatus.FindAsync(id);
            orderStatus2.SetTime = orderStatus.SetTime;

            _context.Entry(orderStatus2).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusExists(id))
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


        // GET: api/MakingOrder/5
        [HttpGet("MAKINGORDERS/{id}")]
        public async Task<ActionResult<OrderThing>> GetOrderStatus(int id)
        {
            var orderStatus = await _context.OrderStatus.FindAsync(id);

            if (orderStatus == null)
            {
                return NotFound();
            }

            // Need to define a object 
            var orderStatus2 = await _context.OrderStatus.ToListAsync();

            if (orderStatus2 == null)
            {
                return NotFound();
            }
            orderStatus2.RemoveAll(t => t.status != "MAKING");
            int i = orderStatus2.Count;
            String j = orderStatus.SetTime;
            OrderThing T = new OrderThing
            { Making = i,
            TIME = j
        };
              
            

            return T;
        }

        // GET: api/OrderStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderStatus>> GetOrdersBeingDone(int id)
        {
            var orderStatus = await _context.OrderStatus.FindAsync(id);

            if (orderStatus == null)
            {
                return NotFound();
            }

            return orderStatus;
        }

        [HttpGet("CSORD/{id}")]
        public async Task<ActionResult<IEnumerable<OrderStatus>>> GetCSOrderStatus(int id)
        {
            var numItems = await _context.OrderStatus.ToListAsync();

            if (numItems == null)
            {
                return NotFound();
            }
            numItems.RemoveAll(t => t.CSUNID != id);
            return numItems;
        }

        [HttpPut("FINISHED/{id}")] 
        public async Task<IActionResult> FinishOrder(int id)
        {
            var orderStatus = await _context.OrderStatus.FindAsync(id);
            if (orderStatus == null)
            {
                return NotFound();
            }
            orderStatus.status = "READY";
            _context.Entry(orderStatus).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                Program.Decrease();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusExists(id))
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

        // PUT: api/OrderStatus/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderStatus(int id, OrderStatus orderStatus)
        {
            if (id != orderStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(orderStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusExists(id))
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

        // POST: api/OrderStatus
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost("{id}")]
        public async Task<ActionResult<OrderStatus>> PostOrderStatus(OrderStatus orderStatus, int id)
        {
            var cart = await _context.CartItems.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
         
            var numItems = await _context.IteminCart.ToListAsync();

            if (numItems == null)
            {
                return NotFound();
            }
            
            numItems.RemoveAll(t => t.CartFK != id);
            
            orderStatus.status = "MAKING";
            orderStatus.CSUNID = cart.CSUNID;
            _context.OrderStatus.Add(orderStatus);
            await _context.SaveChangesAsync();
            // created order
            Program.Increase();
            foreach (IteminCart Item in numItems)
            {
                IteminOrder i = new IteminOrder();
                i.CSUNID = Item.CSUNID;
                i.status = Item.status;
                _context.IteminCart.Remove(Item);
                i.OrderFK = orderStatus.Id;
                _context.IteminOrder.Add(i);

            }
            //put every item in the cart into the order
            
            await _context.SaveChangesAsync();
            _context.CartItems.Remove(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderStatus), new { id = orderStatus.Id }, orderStatus);
        }
        //Count the numbers in the cart
        [HttpGet("GETOrderITEMS/{Id}")]

        public async Task<ActionResult<IEnumerable<IteminOrder>>> GetNum(int Id)
        {

            var numItems = await _context.IteminOrder.ToListAsync();

            if (numItems == null)
            {
                return NotFound();
            }
            numItems.RemoveAll(t => t.OrderFK != Id);
            return numItems;
        }


        // DELETE: api/OrderStatus/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<OrderStatus>> DeleteOrderStatus(int id)
        {
            var orderStatus = await _context.OrderStatus.FindAsync(id);
            if (orderStatus == null)
            {
                return NotFound();
            }

            _context.OrderStatus.Remove(orderStatus);
            await _context.SaveChangesAsync();

            return orderStatus;
        }

        private bool OrderStatusExists(int id)
        {
            return _context.OrderStatus.Any(e => e.Id == id);
        }
    }
}
