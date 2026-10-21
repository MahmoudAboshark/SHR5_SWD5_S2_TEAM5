using System.Text.RegularExpressions;
using ContactBook.ConsoleApp.Data;
using ContactBook.ConsoleApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactBook.ConsoleApp
{
    public class ContactBookApp
    {
        private readonly AppDbContext _db;

        public ContactBookApp(AppDbContext db)
        {
            _db = db;
        }

        public void Run()
        {
            try
            {
                while (true)
                {
                    ShowMenu();
                    var choice = ReadLine().Trim();
                    Console.WriteLine();

                    switch (choice)
                    {
                        case "1": AddContact(); break;
                        case "2": ListContacts(); break;
                        case "3": ViewContact(); break;
                        case "4": ListCategory("Family"); break;
                        case "5": ListCategory("Work"); break;
                        case "6": ListCategory("Friends"); break;
                        case "7": ListCategory("Favorite"); break;
                        case "8": SearchContacts(); break;
                        case "9": UpdateContact(); break;
                        case "10": DeleteContact(); break;
                        case "11": ViewTrash(); break;
                        case "12": RestoreContact(); break;
                        case "13": EmptyTrash(); break;
                        case "14": DeleteAllContacts(); break;
                        case "15": DeleteContactPermanently(); break;
                        case "0":
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            WriteLineCentered("Goodbye!");
                            Console.ResetColor();
                            return;
                        default:
                            PrintWarning("Invalid choice, please pick a number from the menu.", ConsoleColor.White);
                            break;
                    }

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    WriteCentered("Press Enter to continue...");
                    Console.ResetColor();
                    ReadLine();
                }
            }
            catch (EndOfStreamException)
            {
            }
        }

        private static void ShowMenu()
        {
            try { Console.Clear(); } catch (IOException) { }
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLineCentered("==========================================================");
            WriteLineCentered("                     CONTACT BOOK                         ");
            WriteLineCentered("==========================================================");
            Console.ResetColor();

            string[,] menuGrid = new string[,]
            {
                { "1. Add contact", "2. List all", "3. View details" },
                { "4. Family", "5. Work", "6. Friends" },
                { "7. Favorites", "8. Search", "9. Update" },
                { "10. Move to Trash", "11. View Trash", "12. Restore" },
                { "13. Empty Trash", "14. Delete ALL", "15. Delete (Perm)" },
                { "0. EXIT", "", "" }
            };

            int gridWidth = 54;
            int windowWidth = GetSafeWindowWidth();
            int leftPadding = Math.Max(0, (windowWidth - gridWidth) / 2);
            string indent = new string(' ', leftPadding);

            for (int i = 0; i < menuGrid.GetLength(0); i++)
            {
                Console.Write(indent);
                for (int j = 0; j < menuGrid.GetLength(1); j++)
                {
                    string item = menuGrid[i, j];
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (item.StartsWith("1.") || item.StartsWith("2.") || item.StartsWith("3.") || item.StartsWith("11.") || item.StartsWith("12."))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        else if (item.StartsWith("4.") || item.StartsWith("5.") || item.StartsWith("6.") || item.StartsWith("7."))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        else if (item.StartsWith("8.") || item.StartsWith("9."))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }
                        else if (item.StartsWith("10.") || item.StartsWith("13.") || item.StartsWith("14.") || item.StartsWith("15."))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else if (item.StartsWith("0."))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        Console.Write($"{item,-18}");
                        Console.ResetColor();
                    }
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLineCentered("----------------------------------------------------------");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Blue;
            WriteCentered("Choose an option: ");
            Console.ResetColor();
        }

        private void AddContact()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLineCentered("--- Add contact ---");
            WriteLineCentered("Tip: Type 'b' at any prompt to cancel/go back.");
            WriteLineCentered("Name Instructions:");
            WriteLineCentered("- English letters only.");
            WriteLineCentered("- No spaces or numbers allowed.");
            WriteLineCentered("- Must be at least 3 letters.");
            Console.WriteLine();

            var firstName = Ask("First name", required: true, current: null, validate: ValidateName, color: ConsoleColor.Green);
            if (firstName == "BACK_SIGNAL") { PrintCancelled(); return; }

            var lastName = Ask("Last name", required: false, current: null, validate: value => string.IsNullOrEmpty(value) ? null : ValidateName(value), color: ConsoleColor.Green);
            if (lastName == "BACK_SIGNAL") { PrintCancelled(); return; }

            var phone = AskPhoneNumber(ConsoleColor.Green);
            if (phone == "BACK_SIGNAL") { PrintCancelled(); return; }

            var email = AskEmail(ConsoleColor.Green);
            if (email == "BACK_SIGNAL") { PrintCancelled(); return; }

            var category = AskCategory(ConsoleColor.Green);
            if (category == "BACK_SIGNAL") { PrintCancelled(); return; }

            var addressInput = Ask("Address", required: false, current: null, validate: ValidateAddress, color: ConsoleColor.Green);
            if (addressInput == "BACK_SIGNAL") { PrintCancelled(); return; }
            var address = NullIfEmpty(addressInput);

            var contact = new Contact
            {
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone,
                Email = email,
                Category = category,
                Address = address,
                IsDeleted = false
            };

            if (Save(() => _db.Contacts.Add(contact)))
            {
                WriteLineCentered("Contact saved successfully.");
            }
            Console.ResetColor();
        }

        private void ListContacts()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLineCentered("--- All contacts ---");
            Console.ResetColor();

            var contacts = _db.Contacts
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Id)
                .ToList();

            PrintTable(contacts, ConsoleColor.Green);
        }

        private void ListCategory(string category)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLineCentered($"--- {category} Contacts ---");
            Console.ResetColor();

            var contacts = _db.Contacts
                .AsNoTracking()
                .Where(c => c.Category == category && !c.IsDeleted)
                .OrderBy(c => c.Id)
                .ToList();

            PrintTable(contacts, ConsoleColor.Yellow);
        }

        private void ViewContact()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLineCentered("--- Contact details ---");

            var contact = FindContact(track: false, includeDeleted: false, ConsoleColor.Green);
            if (contact is null) { Console.ResetColor(); return; }

            Console.WriteLine();
            
            // Preparing formatted details block so they align vertically as a single neat column
            var lines = new List<string>
            {
                string.Format("{0,-8} : {1}", "Name", FullName(contact)),
                string.Format("{0,-8} : {1}", "Category", contact.Category),
                string.Format("{0,-8} : {1}", "Phone", contact.PhoneNumber),
                string.Format("{0,-8} : {1}", "Email", Or(contact.Email, "-")),
                string.Format("{0,-8} : {1}", "Address", Or(contact.Address, "-")),
                string.Format("{0,-8} : {1:yyyy-MM-dd HH:mm}", "Created", DateTime.SpecifyKind(contact.CreatedAt, DateTimeKind.Utc).ToLocalTime())
            };

            // Calculate max width of details block to center them uniformly as a block
            int blockWidth = lines.Max(l => l.Length);
            int windowWidth = GetSafeWindowWidth();
            int leftPadding = Math.Max(0, (windowWidth - blockWidth) / 2);
            string indent = new string(' ', leftPadding);

            foreach (var line in lines)
            {
                Console.WriteLine(indent + line);
            }

            Console.ResetColor();
        }

        private void SearchContacts()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLineCentered("--- Search contacts ---");
            var term = Ask("Search (name, phone, email, category or address)", required: true, current: null, validate: null, color: ConsoleColor.Blue);
            if (term == "BACK_SIGNAL") { Console.ResetColor(); return; }
            term = term.ToLower();

            var results = _db.Contacts
                .AsNoTracking()
                .Where(c => !c.IsDeleted && (c.FirstName.ToLower().Contains(term)
                         || c.LastName.ToLower().Contains(term)
                         || (c.FirstName.ToLower() + " " + c.LastName.ToLower()).Contains(term)
                         || c.PhoneNumber.Contains(term)
                         || c.Email.ToLower().Contains(term)
                         || c.Category.ToLower().Contains(term)
                         || (c.Address != null && c.Address.ToLower().Contains(term))))
                .OrderBy(c => c.Id)
                .ToList();

            Console.WriteLine();
            PrintTable(results, ConsoleColor.Blue);
            Console.ResetColor();
        }

        private void UpdateContact()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLineCentered("--- Update contact ---");
            WriteLineCentered("Tip: Type 'b' at any prompt to cancel/go back.");
            WriteLineCentered("Name Instructions:");
            WriteLineCentered("- English letters only.");
            WriteLineCentered("- No spaces or numbers allowed.");
            WriteLineCentered("- Must be at least 3 letters.");
            Console.WriteLine();

            var contact = FindContact(track: true, includeDeleted: false, ConsoleColor.Blue);
            if (contact is null) { Console.ResetColor(); return; }

            Console.WriteLine();
            WriteLineCentered("Press Enter to keep the current value, or type - to clear optional field.");

            var fName = Ask("First name", required: true, current: contact.FirstName, validate: ValidateName, color: ConsoleColor.Blue);
            if (fName == "BACK_SIGNAL") { PrintCancelled(); Console.ResetColor(); return; }
            contact.FirstName = fName;

            var lName = Ask("Last name", required: false, current: contact.LastName, validate: value => string.IsNullOrEmpty(value) ? null : ValidateName(value), color: ConsoleColor.Blue);
            if (lName == "BACK_SIGNAL") { PrintCancelled(); Console.ResetColor(); return; }
            contact.LastName = lName;
            
            WriteLineCentered($"Current Phone: {contact.PhoneNumber}");
            WriteCentered("Do you want to update the phone number? (Y/n): ");
            var pChoice = ReadLine().Trim();
            if (pChoice.Equals("b", StringComparison.OrdinalIgnoreCase)) { PrintCancelled(); Console.ResetColor(); return; }
            if (pChoice.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var newPhone = AskPhoneNumber(ConsoleColor.Blue);
                if (newPhone == "BACK_SIGNAL") { PrintCancelled(); Console.ResetColor(); return; }
                contact.PhoneNumber = newPhone;
            }

            WriteLineCentered($"Current Email: {Or(contact.Email, "-")}");
            WriteCentered("Do you want to update the email? (Y/n): ");
            var eChoice = ReadLine().Trim();
            if (eChoice.Equals("b", StringComparison.OrdinalIgnoreCase)) { PrintCancelled(); Console.ResetColor(); return; }
            if (eChoice.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var newEmail = AskEmail(ConsoleColor.Blue);
                if (newEmail == "BACK_SIGNAL") { PrintCancelled(); Console.ResetColor(); return; }
                contact.Email = newEmail;
            }

            WriteLineCentered($"Current Category: {contact.Category}");
            WriteCentered("Do you want to update the category? (Y/n): ");
            var catChoice = ReadLine().Trim();
            if (catChoice.Equals("b", StringComparison.OrdinalIgnoreCase)) { PrintCancelled(); Console.ResetColor(); return; }
            if (catChoice.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var newCat = AskCategory(ConsoleColor.Blue);
                if (newCat == "BACK_SIGNAL") { PrintCancelled(); Console.ResetColor(); return; }
                contact.Category = newCat;
            }

            var adr = Ask("Address", required: false, current: contact.Address ?? string.Empty, validate: ValidateAddress, color: ConsoleColor.Blue);
            if (adr == "BACK_SIGNAL") { PrintCancelled(); Console.ResetColor(); return; }
            contact.Address = NullIfEmpty(adr);

            if (Save())
            {
                WriteLineCentered("Contact updated.");
            }
            Console.ResetColor();
        }

        private void DeleteContact()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLineCentered("--- Delete contact (Move to Trash) ---");

            var contact = FindContact(track: true, includeDeleted: false, ConsoleColor.Red);
            if (contact is null) { Console.ResetColor(); return; }

            WriteLineCentered($"You are about to move to trash: {FullName(contact)} ({contact.PhoneNumber})");
            WriteCentered("Are you sure? (Y/n): ");
            var answer = ReadLine().Trim();

            if (!answer.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                PrintCancelled();
                Console.ResetColor();
                return;
            }

            contact.IsDeleted = true;
            contact.DeletedAt = DateTime.UtcNow;

            if (Save())
            {
                WriteLineCentered("Contact moved to trash successfully.");
            }
            Console.ResetColor();
        }

        private void DeleteContactPermanently()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLineCentered("--- Delete contact permanently ---");

            var contact = FindContact(track: true, includeDeleted: true, ConsoleColor.Red);
            if (contact is null) { Console.ResetColor(); return; }

            WriteLineCentered($"You are about to permanently delete: {FullName(contact)} ({contact.PhoneNumber})");
            WriteCentered("Are you sure? (Y/n): ");
            var answer = ReadLine().Trim();

            if (!answer.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                PrintCancelled();
                Console.ResetColor();
                return;
            }

            if (Save(() => _db.Contacts.Remove(contact)))
            {
                WriteLineCentered("Contact deleted permanently.");
            }
            Console.ResetColor();
        }

        private void ViewTrash()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLineCentered("--- Trash (Deleted Contacts) ---");
            Console.ResetColor();

            var deletedContacts = _db.Contacts
                .AsNoTracking()
                .Where(c => c.IsDeleted)
                .OrderBy(c => c.Id)
                .ToList();

            PrintTable(deletedContacts, ConsoleColor.Green);
        }

        private void RestoreContact()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLineCentered("--- Restore contact from Trash ---");

            List<Contact> deletedContacts = _db.Contacts.Where(c => c.IsDeleted).OrderBy(c => c.Id).ToList();
            if (deletedContacts.Count == 0)
            {
                WriteLineCentered("Trash is empty.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            PrintTable(deletedContacts, ConsoleColor.Green);
            WriteCentered("Enter the display index (Id) of the contact to restore: ");
            
            var input = ReadLine().Trim();
            if (input.Equals("b", StringComparison.OrdinalIgnoreCase)) { Console.ResetColor(); return; }

            if (!int.TryParse(input, out var displayIndex) || displayIndex < 1 || displayIndex > deletedContacts.Count)
            {
                PrintWarning("Please enter a valid index from the list.", ConsoleColor.Green);
                return;
            }

            var selectedContact = deletedContacts[displayIndex - 1];
            var contactToRestore = _db.Contacts.FirstOrDefault(c => c.Id == selectedContact.Id);

            if (contactToRestore is not null)
            {
                contactToRestore.IsDeleted = false;
                contactToRestore.DeletedAt = null;
                if (Save())
                {
                    WriteLineCentered("Contact restored successfully.");
                }
            }
            Console.ResetColor();
        }

        private void EmptyTrash()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLineCentered("--- Empty Trash ---");
            WriteCentered("Are you sure you want to permanently delete all items in the trash? (Y/n): ");
            var answer = ReadLine().Trim();

            if (!answer.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                PrintCancelled();
                Console.ResetColor();
                return;
            }

            try
            {
                var trashItems = _db.Contacts.Where(c => c.IsDeleted).ToList();
                _db.Contacts.RemoveRange(trashItems);
                _db.SaveChanges();
                WriteLineCentered("Trash emptied successfully.");
            }
            catch (Exception ex)
            {
                PrintWarning("Could not empty trash.", ConsoleColor.Red);
                WriteLineCentered(ex.Message);
            }
            Console.ResetColor();
        }

        private void DeleteAllContacts()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLineCentered("--- Delete ALL contacts (Permanent) ---");
            WriteCentered("Are you sure you want to delete ALL contacts permanently? (Y/n): ");
            var answer = ReadLine().Trim();

            if (!answer.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                PrintCancelled();
                Console.ResetColor();
                return;
            }

            try
            {
                _db.Database.ExecuteSqlRaw("DELETE FROM Contacts;");
                _db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name='Contacts';");
                _db.ChangeTracker.Clear();
                WriteLineCentered("All contacts have been deleted permanently.");
            }
            catch (Exception ex)
            {
                PrintWarning("Could not delete all contacts.", ConsoleColor.Red);
                WriteLineCentered(ex.Message);
            }
            Console.ResetColor();
        }

        private Contact? FindContact(bool track, bool includeDeleted, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            var query = includeDeleted ? _db.Contacts : _db.Contacts.Where(c => !c.IsDeleted);
            List<Contact> contacts = query.OrderBy(c => c.Id).ToList();
            
            if (contacts.Count == 0)
            {
                WriteLineCentered("No contacts available.");
                return null;
            }

            Console.WriteLine();
            PrintTable(contacts, color);
            WriteCentered("Enter the display index (Id) of the contact: ");
            
            var input = ReadLine().Trim();
            if (input.Equals("b", StringComparison.OrdinalIgnoreCase)) return null;

            if (!int.TryParse(input, out var displayIndex) || displayIndex < 1 || displayIndex > contacts.Count)
            {
                PrintWarning("Please enter a valid index from the list.", color);
                return null;
            }

            var selectedContact = contacts[displayIndex - 1];
            IQueryable<Contact> dbQuery = track ? _db.Contacts : _db.Contacts.AsNoTracking();
            return dbQuery.FirstOrDefault(c => c.Id == selectedContact.Id);
        }

        private bool Save(Action? change = null)
        {
            try
            {
                change?.Invoke();
                _db.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                PrintWarning("Could not save changes to the database.", ConsoleColor.Red);
                WriteLineCentered(ex.InnerException?.Message ?? ex.Message);
                _db.ChangeTracker.Clear();
                return false;
            }
        }

        private static void PrintTable(List<Contact> contacts, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            if (contacts.Count == 0)
            {
                WriteLineCentered("No contacts found.");
                return;
            }

            WriteLineCentered($"{"Id",-5} {"Name",-18} {"Category",-12} {"Phone",-15} {"Email",-30} {"Address",-35}");
            WriteLineCentered(new string('-', 121));

            int displayIndex = 1;
            foreach (var c in contacts)
            {
                var emailDisplay = string.IsNullOrEmpty(c.Email) ? string.Empty : c.Email.ToLower();
                var addressDisplay = string.IsNullOrEmpty(c.Address) ? "-" : c.Address;
                WriteLineCentered($"{displayIndex,-5} {Cut(FullName(c), 18),-18} {Cut(c.Category, 12),-12} {Cut(c.PhoneNumber, 15),-15} {Cut(emailDisplay, 30),-30} {Cut(addressDisplay, 35),-35}");
                displayIndex++;
            }

            Console.WriteLine();
            WriteLineCentered($"Total: {contacts.Count}");
        }

        private static string AskCategory(ConsoleColor color)
        {
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = color;
                WriteLineCentered("Select Category:");
                WriteLineCentered("1. Family  ");
                WriteLineCentered("2. Work    ");
                WriteLineCentered("3. Friends ");
                WriteLineCentered("4. Favorite");
                WriteLineCentered("5. Other   ");
                WriteCentered("Choose category (1-5): ");

                var input = ReadLine().Trim();
                if (input.Equals("b", StringComparison.OrdinalIgnoreCase)) return "BACK_SIGNAL";

                switch (input)
                {
                    case "1": return "Family";
                    case "2": return "Work";
                    case "3": return "Friends";
                    case "4": return "Favorite";
                    case "5": return "Other";
                    default:
                        PrintWarning("Invalid choice. Pick between 1 and 5.", color);
                        continue;
                }
            }
        }

        private static string AskPhoneNumber(ConsoleColor color)
        {
            string countryCode;
            int expectedDigits;
            string countryName;
            string exampleNumber;

            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = color;
                WriteLineCentered("Select Country:");
                WriteLineCentered("1. Egypt (+20) ");
                WriteLineCentered("2. German (+49)");
                WriteLineCentered("3. Italy (+39) ");
                WriteLineCentered("4. French (+33)");
                WriteLineCentered("5. Lebnan (+961)");
                WriteCentered("Choose country (1-5): ");

                var countryChoice = ReadLine().Trim();
                if (countryChoice.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    return "BACK_SIGNAL";
                }

                switch (countryChoice)
                {
                    case "1":
                        countryName = "Egypt";
                        countryCode = "+20";
                        expectedDigits = 11;
                        exampleNumber = "01148915568";
                        break;
                    case "2":
                        countryName = "German";
                        countryCode = "+49";
                        expectedDigits = 11;
                        exampleNumber = "15123456789";
                        break;
                    case "3":
                        countryName = "Italy";
                        countryCode = "+39";
                        expectedDigits = 10;
                        exampleNumber = "3123456789";
                        break;
                    case "4":
                        countryName = "French";
                        countryCode = "+33";
                        expectedDigits = 10;
                        exampleNumber = "6123456789";
                        break;
                    case "5":
                        countryName = "Lebnan";
                        countryCode = "+961";
                        expectedDigits = 10;
                        exampleNumber = "3123456789";
                        break;
                    default:
                        PrintWarning("Invalid choice. Please pick from 1 to 5.", color);
                        continue;
                }
                break;
            }

            while (true)
            {
                Console.ForegroundColor = color;
                if (countryName == "Egypt")
                {
                    WriteCentered($"Enter phone number (11 digits starting with 0, e.g., {exampleNumber}): ");
                }
                else
                {
                    WriteCentered($"Enter {countryName} phone number ({expectedDigits} digits without country code, e.g., {exampleNumber}): ");
                }

                var rawNumber = ReadLine().Trim();
                if (rawNumber.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    return "BACK_SIGNAL";
                }

                if (!Regex.IsMatch(rawNumber, @"^[0-9]+$"))
                {
                    PrintWarning("Just numbers! Please enter digits only.", color);
                    continue;
                }

                if (rawNumber.Length != expectedDigits)
                {
                    if (countryName == "Egypt")
                    {
                        PrintWarning($"Egyptian phone number must be exactly 11 digits without country code (e.g., {exampleNumber}).", color);
                    }
                    else
                    {
                        PrintWarning($"{countryName} phone must be exactly {expectedDigits} digits without country code (e.g., {exampleNumber}).", color);
                    }
                    continue;
                }

                if (countryName == "Egypt" && !Regex.IsMatch(rawNumber, @"^01[0125][0-9]{8}$"))
                {
                    PrintWarning($"Invalid Egypt number! Must start with 010, 011, 012, or 015 (e.g., {exampleNumber}).", color);
                    continue;
                }

                if (!IsRealisticPhoneNumber(rawNumber))
                {
                    PrintWarning("Invalid number!", color);
                    continue;
                }

                if (countryName == "Egypt")
                {
                    return countryCode + rawNumber.Substring(1);
                }
                else
                {
                    return countryCode + rawNumber;
                }
            }
        }

        private static string AskEmail(ConsoleColor color)
        {
            while (true)
            {
                Console.ForegroundColor = color;
                WriteCentered("Email (optional): ");
                var input = ReadLine().Trim().ToLower();

                if (input.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    return "BACK_SIGNAL";
                }

                if (string.IsNullOrEmpty(input))
                {
                    return string.Empty;
                }

                if (input.EndsWith("@gmail.com") || input.EndsWith("@yahoo.com"))
                {
                    if (Regex.IsMatch(input, @"^[a-z0-9._%+-]+@(gmail\.com|yahoo\.com)$"))
                    {
                        return input;
                    }
                    else
                    {
                        PrintWarning("Invalid email format! Please try again.", color);
                        continue;
                    }
                }

                if (input.Contains("@"))
                {
                    PrintWarning("Invalid domain! Only @gmail.com and @yahoo.com are allowed.", color);
                    continue;
                }

                if (!Regex.IsMatch(input, @"^[a-z0-9._%+-]+$"))
                {
                    PrintWarning("Invalid email username format! Only English letters, numbers, and allowed symbols.", color);
                    continue;
                }

                while (true)
                {
                    Console.ForegroundColor = color;
                    WriteLineCentered("Select Email Domain:");
                    WriteLineCentered("1. @gmail.com");
                    WriteLineCentered("2. @yahoo.com");
                    WriteCentered("Choose domain (1-2): ");

                    var domainChoice = ReadLine().Trim();
                    if (domainChoice.Equals("b", StringComparison.OrdinalIgnoreCase))
                    {
                        return "BACK_SIGNAL";
                    }

                    if (domainChoice == "1")
                    {
                        return input + "@gmail.com";
                    }
                    else if (domainChoice == "2")
                    {
                        return input + "@yahoo.com";
                    }
                    else
                    {
                        PrintWarning("Invalid choice. Pick 1 or 2.", color);
                    }
                }
            }
        }

        private static bool IsRealisticPhoneNumber(string rawNumber)
        {
            if (new string(rawNumber[0], rawNumber.Length) == rawNumber)
            {
                return false;
            }

            int consecutiveCount = 1;
            for (int i = 0; i < rawNumber.Length - 1; i++)
            {
                if (rawNumber[i] == rawNumber[i + 1])
                {
                    consecutiveCount++;
                    if (consecutiveCount > 3)
                    {
                        return false;
                    }
                }
                else
                {
                    consecutiveCount = 1;
                }
            }

            if (Regex.IsMatch(rawNumber, @"(\d{2})\1{2,}"))
            {
                return false;
            }

            return true;
        }

        private static string Ask(string label, bool required, string? current, Func<string, string?>? validate, ConsoleColor color)
        {
            var editing = current is not null;

            while (true)
            {
                Console.ForegroundColor = color;
                var hint = editing ? $" [{current}]" : (required ? "" : " (optional)");
                WriteCentered($"{label}{hint}: ");
                var input = ReadLine().Trim();

                if (input.Equals("b", StringComparison.OrdinalIgnoreCase))
                {
                    return "BACK_SIGNAL";
                }

                if (input.Length == 0)
                {
                    if (editing) return current!;
                    if (required)
                    {
                        PrintWarning($"{label} is required.", color);
                        continue;
                    }
                    return string.Empty;
                }

                if (editing && input == "-")
                {
                    if (required)
                    {
                        PrintWarning($"{label} cannot be empty.", color);
                        continue;
                    }
                    return string.Empty;
                }

                var error = validate?.Invoke(input);
                if (error is not null)
                {
                    continue;
                }

                return input;
            }
        }

        private static string? ValidateName(string value)
        {
            if (!Regex.IsMatch(value, @"^[a-zA-Z]+$"))
            {
                PrintWarning("Invalid name format! - Must use English letters only (No spaces or numbers allowed).", ConsoleColor.Green);
                return "Please re-enter a valid name.";
            }

            if (value.Length < 3)
            {
                PrintWarning("Invalid name format! - Must be at least 3 letters.", ConsoleColor.Green);
                return "Please re-enter a valid name.";
            }

            return null;
        }

        private static string? ValidateAddress(string value)
        {
            if (!Regex.IsMatch(value, @"^[a-zA-Z0-9\s,.-]+$"))
            {
                PrintWarning("Invalid address format! - English letters, numbers, and standard punctuation only (Arabic characters are not allowed).", ConsoleColor.Green);
                return "Please re-enter a valid address.";
            }
            return null;
        }

        private static void PrintWarning(string message, ConsoleColor restoreColor)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            WriteLineCentered(message);
            Console.ForegroundColor = restoreColor;
        }

        private static void PrintCancelled()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLineCentered("Cancelled.");
        }

        private static int GetSafeWindowWidth()
        {
            try
            {
                return Console.WindowWidth > 0 ? Console.WindowWidth : 120;
            }
            catch
            {
                return 120;
            }
        }

        private static void WriteLineCentered(string text)
        {
            int windowWidth = GetSafeWindowWidth();
            int leftPadding = Math.Max(0, (windowWidth - text.Length) / 2);
            Console.WriteLine(new string(' ', leftPadding) + text);
        }

        private static void WriteCentered(string text)
        {
            int windowWidth = GetSafeWindowWidth();
            int leftPadding = Math.Max(0, (windowWidth - text.Length) / 2);
            Console.Write(new string(' ', leftPadding) + text);
        }

        private static string ReadLine() =>
            Console.ReadLine() ?? throw new EndOfStreamException();

        private static string FullName(Contact c) => $"{c.FirstName} {c.LastName}".Trim();

        private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;

        private static string Or(string? value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value;

        private static string Cut(string? value, int max)
        {
            value ??= string.Empty;
            return value.Length <= max ? value : value[..(max - 1)] + "…";
        }
    }
}