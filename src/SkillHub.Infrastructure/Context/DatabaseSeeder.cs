using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.ValueObjects;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Context;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await SeedAsync(context, userManager, roleManager);
    }

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        // Ensure database and tables exist
        if (context.Database.IsRelational())
        {
            await context.Database.EnsureCreatedAsync();
        }

        // 1. Seed Identity roles
        await SeedRolesAsync(roleManager);

        // 2. Seed Instructors (Employees)
        await SeedInstructorsAsync(context, userManager);

        // 3. Seed Workshops
        await SeedWorkshopsAsync(context);

        // 4. Seed Participants and their Enrollments
        await SeedParticipantsAndEnrollmentsAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = [UserRole.Employee.ToString(), UserRole.Participant.ToString()];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task SeedInstructorsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        var instructorsToSeed = new[]
        {
            new
            {
                FirstName = "Andrés Felipe",
                LastName = "Mejía Arango",
                Email = "andres.mejia@correo.com",
                Password = "Instructor2026*",
                Specialty = "Photography",
                IdNumber = "1001001001"
            },
            new
            {
                FirstName = "Rosa Elena",
                LastName = "Barrios Polo",
                Email = "rosa.barrios@correo.com",
                Password = "Instructor2026*",
                Specialty = "Culinary Arts",
                IdNumber = "1001001002"
            },
            new
            {
                FirstName = "Mauricio",
                LastName = "Jiménez Cárdenas",
                Email = "mauricio.jimenez@correo.com",
                Password = "Instructor2026*",
                Specialty = "Public Speaking",
                IdNumber = "1001001003"
            },
            new
            {
                FirstName = "Natalia",
                LastName = "Herrera Castaño",
                Email = "natalia.herrera@correo.com",
                Password = "Instructor2026*",
                Specialty = "Graphic Design",
                IdNumber = "1001001004"
            },
            new
            {
                FirstName = "Hernán Darío",
                LastName = "Quintero Gil",
                Email = "hernan.quintero@correo.com",
                Password = "Instructor2026*",
                Specialty = "Music",
                IdNumber = "1001001005"
            },
            new
            {
                FirstName = "Paola Andrea",
                LastName = "Vargas Mora",
                Email = "paola.vargas@correo.com",
                Password = "Instructor2026*",
                Specialty = "Digital Marketing",
                IdNumber = "1001001006"
            },
            new
            {
                FirstName = "Camila",
                LastName = "Morales Avendaño",
                Email = "cmoralesavendano6@gmail.com",
                Password = "Instructor2026",
                Specialty = "Danza",
                IdNumber = "1041691420"
            }
        };

        foreach (var item in instructorsToSeed)
        {
            var exists = await context.Employees.AnyAsync(e => e.Email.Value == item.Email);
            if (!exists)
            {
                var employee = new Employee(
                    item.FirstName,
                    item.LastName,
                    new Email(item.Email),
                    new IdentificationDocument(IdentificationType.NationalIdentity, item.IdNumber),
                    item.Specialty);

                await context.Employees.AddAsync(employee);
                await context.SaveChangesAsync();

                var appUser = new ApplicationUser(employee.Id, employee.Email.Value);
                var createResult = await userManager.CreateAsync(appUser, item.Password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(appUser, UserRole.Employee.ToString());
                }
            }
        }
    }

    private static async Task SeedWorkshopsAsync(ApplicationDbContext context)
    {
        if (await context.Workshops.AnyAsync())
            return;

        var instructors = await context.Employees.ToListAsync();
        if (!instructors.Any())
            return;

        var phtInstructor = instructors.FirstOrDefault(e => e.Specialty == "Photography") ?? instructors.First();
        var culInstructor = instructors.FirstOrDefault(e => e.Specialty == "Culinary Arts") ?? instructors.First();
        var spkInstructor = instructors.FirstOrDefault(e => e.Specialty == "Public Speaking") ?? instructors.First();
        var dsgInstructor = instructors.FirstOrDefault(e => e.Specialty == "Graphic Design") ?? instructors.First();
        var musInstructor = instructors.FirstOrDefault(e => e.Specialty == "Music") ?? instructors.First();

        var now = DateTime.UtcNow;
        var defaultWorkshops = new[]
        {
            new Workshop(
                "ART-PHT-01",
                "Fotografía Artística e Iluminación de Estudio",
                "Taller práctico de iluminación profesional, composición y retrato de estudio.",
                phtInstructor.Id,
                now.AddDays(10),
                now.AddDays(25),
                15,
                now),
            new Workshop(
                "ART-CUL-02",
                "Alta Cocina Tradicional y Arte Culinario",
                "Técnicas avanzadas de cocina tradicional, manipulación de alimentos y emplatado gourmet.",
                culInstructor.Id,
                now.AddDays(15),
                now.AddDays(33),
                12,
                now),
            new Workshop(
                "HUM-SPK-03",
                "Oratoria Forense, Retórica y Declamación Clásica",
                "Dominio del lenguaje corporal, elocuencia, persuasión y comunicación para el ámbito profesional.",
                spkInstructor.Id,
                now.AddDays(17),
                now.AddDays(40),
                20,
                now),
            new Workshop(
                "ART-DSG-04",
                "Tipografía Monumental y Diseño Gráfico Editorial",
                "Estructuración tipográfica, jerarquías visuales y maquetación editorial contemporánea.",
                dsgInstructor.Id,
                now.AddDays(20),
                now.AddDays(36),
                18,
                now),
            new Workshop(
                "ART-MUS-05",
                "Armonía Clásica y Composición de Cámara",
                "Fundamentos de armonía tonal, contrapunto y arreglos para ensambles de cámara.",
                musInstructor.Id,
                now.AddDays(23),
                now.AddDays(45),
                16,
                now)
        };

        await context.Workshops.AddRangeAsync(defaultWorkshops);
        await context.SaveChangesAsync();
    }

    private static async Task SeedParticipantsAndEnrollmentsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        var studentsToSeed = new[]
        {
            // --- 1. Taller ART-PHT-01 (Fotografía Artística - Capacidad: 15) -> 5 inscritos (10 cupos disponibles) ---
            new
            {
                FirstName = "Sofía",
                LastName = "Montenegro",
                Email = "estudiante.arte@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1098765432",
                PhoneNumber = "3129876543",
                Street = "Calle del Arte 42 # 15-08",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110221",
                WorkshopCode = "ART-PHT-01"
            },
            new
            {
                FirstName = "Daniel Alejandro",
                LastName = "Gómez Pineda",
                Email = "daniel.gomez@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1010001001",
                PhoneNumber = "3001001001",
                Street = "Carrera 7 # 45-20",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "ART-PHT-01"
            },
            new
            {
                FirstName = "Mariana",
                LastName = "Restrepo Duque",
                Email = "mariana.restrepo@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1010001002",
                PhoneNumber = "3001001002",
                Street = "Calle 100 # 19-54",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110111",
                WorkshopCode = "ART-PHT-01"
            },
            new
            {
                FirstName = "Felipe",
                LastName = "Castro Silva",
                Email = "felipe.castro@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1010001003",
                PhoneNumber = "3001001003",
                Street = "Avenida 19 # 104-32",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110121",
                WorkshopCode = "ART-PHT-01"
            },
            new
            {
                FirstName = "Valentina",
                LastName = "Ríos Morales",
                Email = "valentina.rios@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1010001004",
                PhoneNumber = "3001001004",
                Street = "Calle 140 # 12-15",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110131",
                WorkshopCode = "ART-PHT-01"
            },

            // --- 2. Taller ART-CUL-02 (Alta Cocina Tradicional - Capacidad: 12) -> ¡AFORO LLENO! 12 inscritos (0 cupos) ---
            new
            {
                FirstName = "Carlos Eduardo",
                LastName = "Pérez Medina",
                Email = "carlos.perez@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002001",
                PhoneNumber = "3002002001",
                Street = "Calle 85 # 11-50",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110221",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "María José",
                LastName = "Bermúdez Ochoa",
                Email = "mariajose.bermudez@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002002",
                PhoneNumber = "3002002002",
                Street = "Carrera 15 # 93-40",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110221",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Juan Pablo",
                LastName = "Morales Cifuentes",
                Email = "juanpablo.morales@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002003",
                PhoneNumber = "3002002003",
                Street = "Calle 72 # 10-34",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Laura Daniela",
                LastName = "Henao Hoyos",
                Email = "laura.henao@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002004",
                PhoneNumber = "3002002004",
                Street = "Carrera 11 # 82-01",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110221",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Mateo",
                LastName = "Salazar Jaramillo",
                Email = "mateo.salazar@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002005",
                PhoneNumber = "3002002005",
                Street = "Calle 116 # 15-28",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110111",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Andrea Carolina",
                LastName = "Vega Rueda",
                Email = "andrea.vega@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002006",
                PhoneNumber = "3002002006",
                Street = "Carrera 9 # 74-08",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "David Santiago",
                LastName = "Ospina Londoño",
                Email = "david.ospina@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002007",
                PhoneNumber = "3002002007",
                Street = "Calle 127 # 19-45",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110121",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Paula Camila",
                LastName = "Marín Betancur",
                Email = "paula.marin@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002008",
                PhoneNumber = "3002002008",
                Street = "Carrera 13 # 63-39",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Lucas Alberto",
                LastName = "Rincón Forero",
                Email = "lucas.rincon@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002009",
                PhoneNumber = "3002002009",
                Street = "Calle 90 # 14-26",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110221",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Diana Marcela",
                LastName = "Cardona Ortiz",
                Email = "diana.cardona@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002010",
                PhoneNumber = "3002002010",
                Street = "Carrera 19 # 134-12",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110131",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Gabriel Andrés",
                LastName = "Pardo Santamaría",
                Email = "gabriel.pardo@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002011",
                PhoneNumber = "3002002011",
                Street = "Calle 67 # 4A-09",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "ART-CUL-02"
            },
            new
            {
                FirstName = "Sara Sofía",
                LastName = "Builes Arango",
                Email = "sara.builes@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1020002012",
                PhoneNumber = "3002002012",
                Street = "Carrera 7 # 120-20",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110111",
                WorkshopCode = "ART-CUL-02"
            },

            // --- 3. Taller HUM-SPK-03 (Oratoria Forense - Capacidad: 20) -> 5 inscritos (15 cupos disponibles) ---
            new
            {
                FirstName = "Esteban José",
                LastName = "Beltrán Rivas",
                Email = "esteban.beltran@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1030003001",
                PhoneNumber = "3003003001",
                Street = "Calle 53 # 14-56",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "HUM-SPK-03"
            },
            new
            {
                FirstName = "Valeria",
                LastName = "Nieto Soto",
                Email = "valeria.nieto@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1030003002",
                PhoneNumber = "3003003002",
                Street = "Carrera 24 # 39-10",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110311",
                WorkshopCode = "HUM-SPK-03"
            },
            new
            {
                FirstName = "Tomás Enrique",
                LastName = "Carvajal Ruiz",
                Email = "tomas.carvajal@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1030003003",
                PhoneNumber = "3003003003",
                Street = "Calle 45 # 22-18",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110311",
                WorkshopCode = "HUM-SPK-03"
            },
            new
            {
                FirstName = "Manuela",
                LastName = "Arias Zuluaga",
                Email = "manuela.arias@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1030003004",
                PhoneNumber = "3003003004",
                Street = "Carrera 16 # 34-45",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110311",
                WorkshopCode = "HUM-SPK-03"
            },
            new
            {
                FirstName = "Samuel Alejandro",
                LastName = "Cruz Benítez",
                Email = "samuel.cruz@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1030003005",
                PhoneNumber = "3003003005",
                Street = "Calle 63 # 28-30",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110231",
                WorkshopCode = "HUM-SPK-03"
            },

            // --- 4. Taller ART-DSG-04 (Tipografía y Diseño Gráfico - Capacidad: 18) -> 4 inscritos (14 cupos disponibles) ---
            new
            {
                FirstName = "Julián David",
                LastName = "Ortiz Guzmán",
                Email = "julian.ortiz@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1040004001",
                PhoneNumber = "3004004001",
                Street = "Carrera 5 # 26-15",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110311",
                WorkshopCode = "ART-DSG-04"
            },
            new
            {
                FirstName = "Gabriela",
                LastName = "Paz Caicedo",
                Email = "gabriela.paz@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1040004002",
                PhoneNumber = "3004004002",
                Street = "Calle 19 # 4-88",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110311",
                WorkshopCode = "ART-DSG-04"
            },
            new
            {
                FirstName = "Santiago",
                LastName = "Rojas Villamizar",
                Email = "santiago.rojas@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1040004003",
                PhoneNumber = "3004004003",
                Street = "Carrera 8 # 15-42",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110321",
                WorkshopCode = "ART-DSG-04"
            },
            new
            {
                FirstName = "Isabella",
                LastName = "Lozano Peña",
                Email = "isabella.lozano@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1040004004",
                PhoneNumber = "3004004004",
                Street = "Calle 12 # 6-30",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110321",
                WorkshopCode = "ART-DSG-04"
            },

            // --- 5. Taller ART-MUS-05 (Armonía Clásica y Composición - Capacidad: 16) -> 4 inscritos (12 cupos disponibles) ---
            new
            {
                FirstName = "Nicolás",
                LastName = "Bernal Serrano",
                Email = "nicolas.bernal@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1050005001",
                PhoneNumber = "3005005001",
                Street = "Carrera 4 # 11-20",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110321",
                WorkshopCode = "ART-MUS-05"
            },
            new
            {
                FirstName = "Antonella",
                LastName = "Gil Giraldo",
                Email = "antonella.gil@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1050005002",
                PhoneNumber = "3005005002",
                Street = "Calle 10 # 3-15",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110321",
                WorkshopCode = "ART-MUS-05"
            },
            new
            {
                FirstName = "Sebastián",
                LastName = "León Tabares",
                Email = "sebastian.leon@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1050005003",
                PhoneNumber = "3005005003",
                Street = "Carrera 3 # 18-50",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110321",
                WorkshopCode = "ART-MUS-05"
            },
            new
            {
                FirstName = "Luciana",
                LastName = "Vargas Tamayo",
                Email = "luciana.vargas@skillhub.edu",
                Password = "Estudiante2026*",
                IdNumber = "1050005004",
                PhoneNumber = "3005005004",
                Street = "Calle 22 # 5-70",
                City = "Bogotá",
                State = "Cundinamarca",
                PostalCode = "110311",
                WorkshopCode = "ART-MUS-05"
            }
        };

        foreach (var item in studentsToSeed)
        {
            var participant = await context.Participants.FirstOrDefaultAsync(p => p.Email.Value == item.Email);
            if (participant is null)
            {
                participant = new Participant(
                    item.FirstName,
                    item.LastName,
                    new Email(item.Email),
                    new IdentificationDocument(IdentificationType.NationalIdentity, item.IdNumber),
                    new PhoneNumber("+57", item.PhoneNumber),
                    new Address(item.Street, item.City, item.State, item.PostalCode, "Colombia"));

                await context.Participants.AddAsync(participant);
                await context.SaveChangesAsync();
            }

            var appUser = await userManager.FindByEmailAsync(item.Email);
            if (appUser is null)
            {
                appUser = new ApplicationUser(participant.Id, participant.Email.Value);
                var createResult = await userManager.CreateAsync(appUser, item.Password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(appUser, UserRole.Participant.ToString());
                }
            }

            if (!string.IsNullOrWhiteSpace(item.WorkshopCode))
            {
                var workshop = await context.Workshops.FirstOrDefaultAsync(w => w.Code == item.WorkshopCode);
                if (workshop is not null)
                {
                    var isEnrolled = await context.Enrollments.AnyAsync(e =>
                        e.ParticipantId == participant.Id &&
                        e.WorkshopId == workshop.Id &&
                        e.Status == EnrollmentStatus.Active);

                    if (!isEnrolled)
                    {
                        var enrollment = new Enrollment(participant.Id, workshop.Id);
                        await context.Enrollments.AddAsync(enrollment);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
