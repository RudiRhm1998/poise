namespace poise.services.Roles;

public enum Permissions
{
	ReadProject = 10,
	CreateProject = 11,
	UpdateProject = 12,
	DeleteProject = 13,

	ReadCustomer = 20,
	CreateCustomer = 21,
	UpdateCustomer = 22,
	DeleteCustomer = 23,

	ReadUser = 30,
	UpdateUser = 31,
	CreateUser = 32,

	ReadRole = 40,
	CreateRole = 41,
	UpdateRole = 42,
	DeleteRole = 43,

	ReadActivity = 50,
	CreateActivity = 51,
	UpdateActivity = 52,
	DeleteActivity = 53,

	ReadBooking = 60,
	CreateBooking = 61,
	UpdateBooking = 62,
	DeleteBooking = 63,

	ReadTeam = 70,
	CreateTeam = 71,
	UpdateTeam = 72,
	DeleteTeam = 73,
}