﻿using AutoMapper;
using BookStore.Application.IService;
using BookStore.Core.FilterModel;
using BookStore.Core.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrderController : ControllerBase
	{
		private readonly IOrderReponsitory _orderReponsitory;
		private readonly IMapper _mapper;
		private readonly IOrderService _orderService;
		public OrderController(IOrderReponsitory orderReponsitory, IMapper mapper, IOrderService orderService)
		{
			_orderReponsitory = orderReponsitory;
			_mapper = mapper;
			_orderService = orderService;
		}
		[HttpGet]
		public async Task<IActionResult> GetListOrder()
		{
			return Ok(await _orderService.GetListOrder());
		}
		[HttpGet("{idUser}")]
		public async Task<IActionResult> GetBooksInOrder(int idUser)
		{
			var books = await _orderService.GetBooksInOrder(idUser);
			return Ok(books);
		}
		[HttpPost]
		public async Task<IActionResult> AddBooksIntoOrder([FromBody] OrderFilterModel filter)
		{
			await _orderService.AddBooksIntoOrder(filter);
			return Ok();
		}
		[HttpDelete("{idOrder}")]
		public async Task<IActionResult> Delete(int idOrder)
		{
			await _orderService.Delete(idOrder);
			return Ok();
		}
	}
	
}
