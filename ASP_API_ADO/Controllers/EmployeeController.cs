using ASP_API_ADO.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ASP_API_ADO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly string _conString;
        public EmployeeController(IConfiguration configuration)
        {

            _conString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }



        private async Task<List<Employee>> GetAll()
        {
            List<Employee> employees = new List<Employee>();

            using SqlConnection con = new SqlConnection(_conString);
            string query = "SELECT * FROM Employees";
            SqlCommand cmd = new SqlCommand(query, con);

            await con.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                Employee employee = new Employee
                {
                    Id = (int)reader["Id"],
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Gender = reader["Gender"].ToString(),
                    Salary = (int)reader["Salary"]
                };
                employees.Add(employee);
            }

            return employees;
        }

        [HttpGet("employee-list")]
        public async Task<IActionResult> Employees()
        {
            var employees = await GetAll();

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Employee(int id)
        {
            var employee = (await GetAll()).Find(x => x.Id == id);

            return Ok(employee);
        
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] Employee employee)
        {
            using SqlConnection con = new SqlConnection(_conString);

            string query = $"INSERT INTO Employees (FirstName, LastName, Gender, Salary) VALUES (@firstName, @lastName, @gender, @salary)";
            SqlCommand cmd = new SqlCommand(query , con);
            cmd.Parameters.AddWithValue("@firstName", employee.FirstName);
            cmd.Parameters.AddWithValue("@lastName", employee.LastName);
            cmd.Parameters.AddWithValue("@gender", employee.Gender);
            cmd.Parameters.AddWithValue("@salary", employee.Salary);

            await con.OpenAsync();

            var result = await cmd.ExecuteNonQueryAsync();
            if(result < 1)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] Employee employee)
        {
            using SqlConnection con = new SqlConnection(_conString);

            string query = $"UPDATE Employees SET FirstName = @firstName, LastName = @lastName, Gender = @gender, Salary = @salary WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", employee.Id);
            cmd.Parameters.AddWithValue("@firstName", employee.FirstName);
            cmd.Parameters.AddWithValue("@lastName", employee.LastName);
            cmd.Parameters.AddWithValue("@gender", employee.Gender);
            cmd.Parameters.AddWithValue("@salary", employee.Salary);

            await con.OpenAsync();

            var result = await cmd.ExecuteNonQueryAsync();
            if (result < 1)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            var employee = (await GetAll()).FirstOrDefault(x => x.Id == id);
            
            using SqlConnection con = new SqlConnection(_conString);
            string query = $"DELETE FROM Employees WHERE Id = @id";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);

            if(con.State == ConnectionState.Open)
            {
                await con.CloseAsync();
            }

            await con.OpenAsync();
            
            var result = await cmd.ExecuteNonQueryAsync();
            
            if(result < 1)
            {
                return BadRequest(result);
            }

            return Ok($"Employee {employee.FirstName} was deleted!");
        }
    }
}
