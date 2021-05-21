﻿using BookStore.Core.Entity;
using BookStore.Core.Repository;
using BookStore.Core.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Database.Reponsitory
{
	public class OrderResponsitory : IOrderReponsitory
	{
		private readonly ApplicationDbContext _context;
		public OrderResponsitory(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task Commit() => await _context.SaveChangesAsync();
		public async Task AddOrder(Order order) => await _context.Orders.AddAsync(order);
		public async Task<ListItemResponse<Order>> GetListOrder()
		{
			IQueryable<Order> orders = _context.Orders;
			return new ListItemResponse<Order>()
			{
				Data = await orders.ToListAsync(),
				Count = await orders.CountAsync()
			};
		}

		
	}
}
