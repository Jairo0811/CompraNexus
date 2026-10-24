document.addEventListener("DOMContentLoaded", function () {
  var host = document.getElementById("order-lines");
  var add = document.getElementById("add-line");
  var template = document.getElementById("line-template");
  if (!host || !add || !template) return;
  add.addEventListener("click", function () {
    host.appendChild(template.content.cloneNode(true));
  });
  host.addEventListener("click", function (event) {
    if (!event.target.classList.contains("remove-line")) return;
    if (host.querySelectorAll(".line-row").length <= 1) {
      alert("Una orden debe tener al menos un artículo.");
      return;
    }
    event.target.closest(".line-row").remove();
  });
  var department = document.getElementById("department");
  var employee = document.getElementById("employee");
  if (department && employee) {
    function filterEmployees() {
      var choice = department.value;
      Array.from(employee.options).forEach(function (option) {
        if (!option.value) return;
        option.hidden = option.dataset.department !== choice;
      });
      if (employee.selectedOptions.length && employee.selectedOptions[0].hidden) {
        employee.value = "";
      }
    }
    department.addEventListener("change", filterEmployees);
    filterEmployees();
  }
});
