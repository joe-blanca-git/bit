using apiBit.DTOs;
using apiBit.Interfaces;
using apiBit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace apiBit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Dev")]
    [Produces("application/json")]
    public class AppManagerController : ControllerBase
    {
        private readonly IAppManagerService _service;

        public AppManagerController(IAppManagerService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna a árvore completa de Apps, Menus e Submenus.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Application>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllApps();
            return Ok(result);
        }

        /// <summary>
        /// Cria um novo Aplicativo (Ex: Bit Admin).
        /// </summary>
        [HttpPost("app")]
        [ProducesResponseType(typeof(Application), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateApp([FromBody] ApplicationDto model)
        {
            var result = await _service.CreateApp(model);
            return Ok(result);
        }

        /// <summary>
        /// Cria um Menu Principal (Ex: Financeiro).
        /// </summary>
        [HttpPost("menu")]
        [ProducesResponseType(typeof(ApplicationMenu), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateMenu([FromBody] MenuDto model)
        {
            try 
            {
                var result = await _service.CreateMenu(model);
                return Ok(result);
            }
            catch(Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        /// <summary>
        /// Cria um SubMenu (Ex: Contas a Pagar).
        /// </summary>
        /// <remarks>Necessário informar o ParentMenuId.</remarks>
        [HttpPost("submenu")]
        [ProducesResponseType(typeof(ApplicationSubMenu), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateSubMenu([FromBody] MenuDto model)
        {
            try
            {
                var result = await _service.CreateSubMenu(model);
                return Ok(result);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("app/{id}")]
        public async Task<IActionResult> DeleteApp(Guid id) 
        {
            if(await _service.DeleteApp(id)) return NoContent();
            return NotFound();
        }

        [HttpDelete("menu/{id}")]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            if (await _service.DeleteMenu(id)) return NoContent();
            return NotFound();
        }

        [HttpDelete("submenu/{id}")]
        public async Task<IActionResult> DeleteSubMenu(Guid id)
        {
            if (await _service.DeleteSubMenu(id)) return NoContent();
            return NotFound();
        }

        /// <summary>
        /// Define todas as permissões (Apps, Menus, Subs) de um Plano de uma vez.
        /// </summary>
        /// <remarks>Atenção: Envie a lista COMPLETA. O que não for enviado será removido do plano.</remarks>
        [HttpPost("set-permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SetPermissions([FromBody] PlanPermissionDto model)
        {
            try
            {
                await _service.SetPlanPermissions(model);
                return Ok(new { message = "Permissões atualizadas com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}