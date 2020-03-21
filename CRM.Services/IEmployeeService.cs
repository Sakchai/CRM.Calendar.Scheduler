using CRM.Model;
using System;
using System.Collections.Generic;

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
        SmEmployee GetEmployeeByID(string id);
        SmEmployee GetEmployeeByFacebook(string facebook);
        IList<SmEmployee> GetEmployeesList();
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