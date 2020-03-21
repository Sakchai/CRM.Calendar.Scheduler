using CRM.Model;
using System;

namespace CRM.Services
{
    /// <summary>
    /// Employee service interface
    /// </summary>
    public partial interface IEmployeeService
    {
        /// <summary>
        /// Gets an employee by employee identifier
        /// </summary>
        /// <param name="email">Employee identifier</param>
        /// <returns>Employee</returns>
        SmEmployee GetEmployeeByEmail(string email);


        /// <summary>
        /// Marks employee as deleted 
        /// </summary>
        /// <param name="employee">Employee</param>
        void DeleteEmployee(SmEmployee employee);

       

        /// <summary>
        /// Inserts an employee
        /// </summary>
        /// <param name="employee">Employee</param>
        void InsertEmployee(SmEmployee employee);

        /// <summary>
        /// Updates the employee
        /// </summary>
        /// <param name="employee">Employee</param>
        void UpdateEmployee(SmEmployee employee);



    }
}