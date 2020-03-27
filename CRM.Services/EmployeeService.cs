using System;
using System.Collections.Generic;
using System.Linq;
using CRM.Model;

namespace CRM.Services
{
    /// <summary>
    /// Employee service
    /// </summary>
    public partial class EmployeeService : IEmployeeService
    {
        #region Fields


        private readonly IRepository<SmEmployee> _employeeRepository;

        #endregion

        #region Ctor

        public EmployeeService(IRepository<SmEmployee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an employee by employee identifier
        /// </summary>
        /// <param name="employeeId">Employee identifier</param>
        /// <returns>Employee</returns>
        public virtual SmEmployee GetEmployeeByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = _employeeRepository.Table;
            return query.Where(x => x.Email == email).FirstOrDefault();

        }

      
        /// <summary>
        /// Marks employee as deleted 
        /// </summary>
        /// <param name="employee">Employee</param>
        public virtual void DeleteEmployee(SmEmployee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            employee.IsDelete = true;
            UpdateEmployee(employee);

        }

        /// <summary>
        /// Inserts an employee
        /// </summary>
        /// <param name="employee">Employee</param>
        public virtual void InsertEmployee(SmEmployee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            _employeeRepository.Insert(employee);

        }

        /// <summary>
        /// Updates the employee
        /// </summary>
        /// <param name="employee">Employee</param>
        public virtual void UpdateEmployee(SmEmployee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            _employeeRepository.Update(employee);

        }

        public SmEmployee GetEmployeeByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var query = _employeeRepository.Table;
            return query.Where(x => x.EmpId == id).FirstOrDefault();
        }

        public SmEmployee GetEmployeeByFacebook(string facebook)
        {
            if (string.IsNullOrWhiteSpace(facebook))
                return null;

            var query = _employeeRepository.Table;
            return query.Where(x => x.Facebook == facebook).FirstOrDefault();
        }

        public IList<SmEmployee> GetEmployeesList()
        {
            var query = _employeeRepository.Table;
            return query.Where(x => !x.IsDelete.Value).ToList();
            //&& x.Email == "th.sakchai@gmail.com").ToList();
        }



        #endregion
    }
}