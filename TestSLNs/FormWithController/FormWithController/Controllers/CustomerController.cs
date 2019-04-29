using FormWithController.Models;
using FormWithController.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormWithController.Controllers
{
    class CustomerController
    {
        public Customer Model { get; set; } = new Customer();
        public CustomerView1 View { get; set; }

    }
}
