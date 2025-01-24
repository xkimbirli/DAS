using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Appointment.Models;
using Appointment.Helpers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
namespace Appointment.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

       
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {


            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Message"] = "All fields are required.";
                return View();
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Retrieve user data based on email
                    var query = "SELECT * FROM users WHERE email = @Email";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())  // Ensure that there is data to read
                            {
                                var userId = reader["id"]; // Keep the original format
                                var userType = reader["type"];
                                var dbPasswordHash = reader["password"] as string;
                                ViewData["Message"] = userId;

                                if (!string.IsNullOrEmpty(dbPasswordHash) && PasswordHelper.VerifyPassword(dbPasswordHash, password))
                                {
                                    if (userId != DBNull.Value && userType != DBNull.Value)
                                    {

                                        HttpContext.Session.SetString("userId", userId.ToString() ?? "defaultUserId");
                                        HttpContext.Session.SetString("userType", userType.ToString() ?? "defaultUserType");

                                        ViewData["Message"] = "Login successful!";


                                        if (userType.ToString() == "3")
                                        {
                                            // Admin
                                            return Redirect("/Admin/Employees");
                                        }
                                        else if (userType.ToString() == "1")
                                        {
                                            // Dentist
                                            return Redirect("/Admin/PatientSchedule");
                                        }
                                        else if (userType.ToString() == "2")
                                        {
                                            // Receptionist
                                            return Redirect("/Admin/DentistSchedule");
                                        }
                                        else
                                        {
                                            // Client
                                            return Redirect("/");
                                        }
                                        

                                    } else
                                    {
                                        ViewData["Message"] = "Login error!";
                                    }
                                  
                                } else
                                {
                                    ViewData["Message"] = "Login error!";
                                }

                            }
                            else
                            {
                                // Handle case where reader didn't return any rows
                                ViewData["Message"] = "Invalid email or password.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}, Stack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An error occurred during login.";
            }

            return View();
        }




        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string firstname, string middlename, string lastname, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(firstname) || string.IsNullOrWhiteSpace(middlename) || string.IsNullOrWhiteSpace(lastname) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewData["Message"] = "All fields are required.";
                return View();
            }

            var passwordHash = PasswordHelper.HashPassword(password);

            var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the email already exists in the database
                    var checkQuery = "SELECT COUNT(1) FROM users WHERE email = @Email";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Email", email);
                        var existingCount = (int)checkCommand.ExecuteScalar();

                        if (existingCount > 0)
                        {
                            ViewData["Message"] = "An account with this email already exists.";
                            return View();
                        }
                    }

                    // If no duplicate is found, proceed to insert the new user
                    var insertQuery = @"
                INSERT INTO users (firstname, middlename, lastname, email, password, type) 
                VALUES (@FirstName, @MiddleName, @LastName, @Email, @PasswordHash, @Type)";

                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@FirstName", firstname);
                        insertCommand.Parameters.AddWithValue("@MiddleName", middlename);
                        insertCommand.Parameters.AddWithValue("@LastName", lastname);
                        insertCommand.Parameters.AddWithValue("@Email", email);
                        insertCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        insertCommand.Parameters.AddWithValue("@Type", 0);

                        insertCommand.ExecuteNonQuery();
                    }
                }

                ViewData["Message"] = "Registration successful!";
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during registration: {sqlEx.Message} \nStack Trace: {sqlEx.StackTrace}");
                ViewData["Message"] = $"Database error: {sqlEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"General Error during registration: {ex.Message} \nStack Trace: {ex.StackTrace}");
                ViewData["Message"] = "An unexpected error occurred during registration.";
            }

            return View();
        }
        public IActionResult Logout()
        {
            // Destroy the session
            HttpContext.Session.Clear();

            // Redirect the user to the home page or login page
            return RedirectToAction("Index", "Home"); // Or your login page
        }
    }
}
