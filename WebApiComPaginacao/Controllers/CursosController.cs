using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using WebApiComPaginacao.Models;
using X.PagedList;

namespace WebApiComPaginacao.Controllers
{
    public class CursosController : ApiController
    {
        private CursoDbContext db = new CursoDbContext();

        //por padrão o asp net API
        //tipos primitivos vem na url
        //tipos complexos vem no corpo da requisição
        //IHttpActionResult - diferentes retornos possiveis implementam essa interface como por exemplo um badrequest ....
        public IHttpActionResult PostCurso(Curso curso)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            db.Cursos.Add(curso);
            db.SaveChanges();

            //retorna com o curso criado
            return CreatedAtRoute("DefaultApi", new { id = curso.Id }, curso);
        }

        [HttpGet]
        public IHttpActionResult CursoPorId(int id)
        {
            var contexto = Request;

            if (id <= 0)
                return BadRequest("O id deve ser um número maior que zero.");

            var curso = db.Cursos.Find(id);

            if (curso == null)
                return NotFound(); //recurso padrão do webapi - implemnetam a interface IHttpActionResult

            return Ok(curso);
        }

        public IHttpActionResult PutCurso(int id, Curso curso)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //se o id do curso informado na url é o mesmo que esta sendo enviado no objeto que esta vindo no corpo
            if (id != curso.Id)
                return BadRequest("O id informado na URL é diferente do id informado no corpo da requisição.");

            if (db.Cursos.Count(c => c.Id == id) == 0)
                return NotFound();

            db.Entry(curso).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult DeleteCurso(int id)
        {
            if (id <= 0)
                return BadRequest("O id deve ser  um número maior que zero.");


            var curso = db.Cursos.Find(id);

            if (curso == null)
                return NotFound();

            db.Cursos.Remove(curso);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }


        /// <summary>
        /// get paginado
        /// atribuimos valores padrão caso não seja passado na url
        /// </summary>
        /// <param name="pagina"></param>
        /// <param name="tamanhoPagina"></param>
        /// <returns></returns>
        public IHttpActionResult GetCursos(int pagina = 1, int tamanhoPagina = 10)
        {
            //validação
            if (pagina <= 0 || tamanhoPagina <= 0)
                return BadRequest("Os parâmetros pagina e tamanhoPagina devem ser maiores que zero.");
            //limitando o tamanho maximo de paginas
            if (tamanhoPagina > 10)
                return BadRequest("O tamanho máximo de página permitido é 10.");

            //verifica se a pagina que esta sendo solicitada não é maior que o total de paginas que eu tenho
            //Math.Ceiling arredonda para cima a divisão (valor decimal) - caso de 1.5 ele retorna 2
            int totalPaginas = (int)Math.Ceiling(db.Cursos.Count() / Convert.ToDecimal(tamanhoPagina));

            if (pagina > totalPaginas)
                return BadRequest("A página solicitada não existe.");

            //http preve cabeçalhos adicionais - customizados de acordo com a necessidade
            //adiciona nos cabeçalhos da resposta 
            HttpContext.Current.Response.AddHeader("X-Pagination-TotalPages", totalPaginas.ToString());

            //indica que existe no minimo uma pagina antes dessa
            if (pagina > 1)
                HttpContext.Current.Response.AddHeader("X-Pagination-PreviousPage",
                        Url.Link("DefaultApi", new
                        {
                            pagina = pagina - 1,
                            tamanhoPagina = tamanhoPagina
                        }));

            //se a pagina atual é menor que a ultima
            if (pagina < totalPaginas)
                HttpContext.Current.Response.AddHeader("X-Pagination-NextPage",
                     Url.Link("DefaultApi", new
                     {
                         pagina = pagina + 1,
                         tamanhoPagina = tamanhoPagina
                     }));

            //resultado paginado - 
            //Skip é utilizada para avançar um determinado número de itens da lista e o Take indica quantos itens serão recuperados da lista
            //para utilizar o skip é necessário que esteja ordenado
            //aqui podemos ter um gargalo devido falta de indice na datapublicação
            var cursos = db.Cursos.OrderBy(c => c.Id).ToPagedList(pagina, tamanhoPagina);
            
            return Ok(cursos);
        }
    }
}
