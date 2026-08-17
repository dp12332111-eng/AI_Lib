using LibraryRecommendation.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryRecommendation.Api.Data;

/// <summary>
/// Seeds a demo library. Descriptions are deliberately multi-sentence and vocabulary-rich:
/// the TF-IDF engine has nothing to work with if every blurb is one bland line.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(LibraryDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        if (!await context.Books.AnyAsync(cancellationToken))
        {
            context.Books.AddRange(BuildBooks());
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Users.AnyAsync(cancellationToken))
        {
            var books = await context.Books.ToDictionaryAsync(b => b.Title, b => b.Id, cancellationToken);
            var users = BuildUsers();
            context.Users.AddRange(users);
            await context.SaveChangesAsync(cancellationToken);

            var usersByName = users.ToDictionary(u => u.Name, u => u.Id);
            context.Ratings.AddRange(BuildRatings(usersByName, books));
            context.ReadingHistory.AddRange(BuildReadingHistory(usersByName, books));
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static List<Book> BuildBooks() =>
    [
        // ---------------- Science Fiction ----------------
        new Book
        {
            Title = "Dune",
            Author = "Frank Herbert",
            Genre = "Science Fiction",
            Description = "On the desert planet Arrakis, noble houses wage war over a spice that grants " +
                          "interstellar navigation and prophetic vision. A young heir joins the desert tribes " +
                          "and becomes the messiah of an ecological and religious revolution.",
            Rating = 4.6,
            PublishedYear = 1965
        },
        new Book
        {
            Title = "Neuromancer",
            Author = "William Gibson",
            Genre = "Science Fiction",
            Description = "A burned-out console cowboy is hired for one last hack against a corporate " +
                          "artificial intelligence orbiting Earth. Cyberspace, black clinics and rogue AI " +
                          "collide in the novel that defined cyberpunk.",
            Rating = 4.2,
            PublishedYear = 1984
        },
        new Book
        {
            Title = "Foundation",
            Author = "Isaac Asimov",
            Genre = "Science Fiction",
            Description = "A mathematician predicts the collapse of a galactic empire and founds a colony to " +
                          "shorten the coming dark age. Across centuries, psychohistory and political " +
                          "manoeuvring shape the fate of an interstellar civilisation.",
            Rating = 4.4,
            PublishedYear = 1951
        },
        new Book
        {
            Title = "I, Robot",
            Author = "Isaac Asimov",
            Genre = "Science Fiction",
            Description = "Linked stories probe the three laws of robotics through robots that obey the letter " +
                          "of their programming and violate its spirit. A robopsychologist untangles machine " +
                          "logic, artificial intelligence and human assumptions.",
            Rating = 4.2,
            PublishedYear = 1950
        },
        new Book
        {
            Title = "The Left Hand of Darkness",
            Author = "Ursula K. Le Guin",
            Genre = "Science Fiction",
            Description = "An envoy arrives on an icebound planet whose inhabitants have no fixed gender. " +
                          "Diplomacy, exile and a brutal trek across the glacier force him to rethink " +
                          "loyalty, politics and what makes a person.",
            Rating = 4.3,
            PublishedYear = 1969
        },
        new Book
        {
            Title = "Snow Crash",
            Author = "Neal Stephenson",
            Genre = "Science Fiction",
            Description = "A pizza-delivering hacker and a teenage courier chase a digital drug through a " +
                          "privatised America and its virtual metaverse. Ancient linguistics, viruses and " +
                          "corporate franchises tangle into one conspiracy.",
            Rating = 4.1,
            PublishedYear = 1992
        },
        new Book
        {
            Title = "The Martian",
            Author = "Andy Weir",
            Genre = "Science Fiction",
            Description = "Stranded alone on Mars after his crew evacuates, an astronaut engineer must grow " +
                          "food and rig communications from spare parts. Survival becomes a sequence of " +
                          "chemistry, botany and orbital mechanics problems solved under deadline.",
            Rating = 4.5,
            PublishedYear = 2011
        },
        new Book
        {
            Title = "Klara and the Sun",
            Author = "Kazuo Ishiguro",
            Genre = "Science Fiction",
            Description = "An artificial friend powered by solar energy watches the human family that buys her " +
                          "and tries to understand love and illness. The novel asks what an artificial " +
                          "intelligence can know about the people it serves.",
            Rating = 4.0,
            PublishedYear = 2021
        },

        // ---------------- Fantasy ----------------
        new Book
        {
            Title = "The Hobbit",
            Author = "J.R.R. Tolkien",
            Genre = "Fantasy",
            Description = "A comfortable hobbit is swept into a quest with thirteen dwarves to reclaim a " +
                          "mountain hoard from a dragon. Wizards, trolls, elves and a riddling creature in " +
                          "the dark stand between him and home.",
            Rating = 4.7,
            PublishedYear = 1937
        },
        new Book
        {
            Title = "The Name of the Wind",
            Author = "Patrick Rothfuss",
            Genre = "Fantasy",
            Description = "An innkeeper recounts how he became a legendary magician, musician and thief. " +
                          "His story moves from a travelling troupe to a university of arcane sympathy where " +
                          "magic is studied like a science.",
            Rating = 4.5,
            PublishedYear = 2007
        },
        new Book
        {
            Title = "A Wizard of Earthsea",
            Author = "Ursula K. Le Guin",
            Genre = "Fantasy",
            Description = "A gifted boy on an island of wizards summons a shadow he cannot name and must hunt " +
                          "it across the archipelago. Learning true names, balance and restraint matters more " +
                          "than raw magical power.",
            Rating = 4.3,
            PublishedYear = 1968
        },
        new Book
        {
            Title = "The Fifth Season",
            Author = "N.K. Jemisin",
            Genre = "Fantasy",
            Description = "On a continent wracked by apocalyptic seismic ages, people who can quell earthquakes " +
                          "are enslaved for their power. Three women's stories converge as a mother searches " +
                          "for her stolen daughter through a dying world.",
            Rating = 4.4,
            PublishedYear = 2015
        },
        new Book
        {
            Title = "Mistborn: The Final Empire",
            Author = "Brandon Sanderson",
            Genre = "Fantasy",
            Description = "In an ash-covered empire ruled by an immortal lord, a street thief discovers she can " +
                          "burn metals for magical power. A crew of thieves plots a heist that doubles as a " +
                          "rebellion against a god-emperor.",
            Rating = 4.5,
            PublishedYear = 2006
        },
        new Book
        {
            Title = "Piranesi",
            Author = "Susanna Clarke",
            Genre = "Fantasy",
            Description = "A man lives in an endless labyrinth of marble halls flooded by tides and populated " +
                          "by statues. As he catalogues the house, letters and memories reveal how he came to " +
                          "this strange world.",
            Rating = 4.2,
            PublishedYear = 2020
        },

        // ---------------- Mystery & Crime ----------------
        new Book
        {
            Title = "The Hound of the Baskervilles",
            Author = "Arthur Conan Doyle",
            Genre = "Mystery",
            Description = "A spectral hound is said to stalk the heirs of a Devon estate across the fog-bound " +
                          "moor. Sherlock Holmes sends Watson ahead to gather evidence while he pursues the " +
                          "rational explanation behind the legend.",
            Rating = 4.4,
            PublishedYear = 1902
        },
        new Book
        {
            Title = "And Then There Were None",
            Author = "Agatha Christie",
            Genre = "Mystery",
            Description = "Ten strangers are lured to an island mansion and accused of murders they were never " +
                          "punished for. One by one they die in the order of a nursery rhyme, and the killer " +
                          "is among them.",
            Rating = 4.6,
            PublishedYear = 1939
        },
        new Book
        {
            Title = "The Girl with the Dragon Tattoo",
            Author = "Stieg Larsson",
            Genre = "Mystery",
            Description = "A disgraced financial journalist and a brilliant, damaged hacker investigate a woman " +
                          "who vanished from a private island forty years ago. Corporate corruption and family " +
                          "violence surface as the cold case reopens.",
            Rating = 4.2,
            PublishedYear = 2005
        },
        new Book
        {
            Title = "Gone Girl",
            Author = "Gillian Flynn",
            Genre = "Mystery",
            Description = "A wife disappears on her fifth wedding anniversary and her husband becomes the prime " +
                          "suspect. Alternating diaries and interrogations expose a marriage built on " +
                          "performance and revenge.",
            Rating = 4.1,
            PublishedYear = 2012
        },
        new Book
        {
            Title = "The Big Sleep",
            Author = "Raymond Chandler",
            Genre = "Mystery",
            Description = "A private detective takes a blackmail case for a dying general and finds pornography " +
                          "rackets, gambling debts and corpses. Los Angeles glamour rots under the surface of " +
                          "every interview he conducts.",
            Rating = 4.2,
            PublishedYear = 1939
        },
        new Book
        {
            Title = "The Thursday Murder Club",
            Author = "Richard Osman",
            Genre = "Mystery",
            Description = "Four residents of a retirement village meet weekly to reinvestigate cold cases for " +
                          "fun. When a developer is murdered on their doorstep, their amateur detective work " +
                          "outpaces the police investigation.",
            Rating = 4.0,
            PublishedYear = 2020
        },

        // ---------------- Historical Fiction ----------------
        new Book
        {
            Title = "Wolf Hall",
            Author = "Hilary Mantel",
            Genre = "Historical Fiction",
            Description = "Thomas Cromwell rises from a blacksmith's son to the most powerful advisor in Henry " +
                          "VIII's court. The Reformation, a contested royal marriage and courtly politics are " +
                          "seen through his patient, calculating eye.",
            Rating = 4.2,
            PublishedYear = 2009
        },
        new Book
        {
            Title = "All the Light We Cannot See",
            Author = "Anthony Doerr",
            Genre = "Historical Fiction",
            Description = "A blind French girl and a German radio operator move towards each other through the " +
                          "occupation and bombardment of a walled coastal town. Wartime broadcasts, a cursed " +
                          "diamond and small acts of courage bind their stories.",
            Rating = 4.4,
            PublishedYear = 2014
        },
        new Book
        {
            Title = "The Book Thief",
            Author = "Markus Zusak",
            Genre = "Historical Fiction",
            Description = "Narrated by Death, a foster girl in Nazi Germany steals books and reads them in a " +
                          "basement shelter. Her family hides a Jewish refugee while the war closes in on " +
                          "their small street.",
            Rating = 4.5,
            PublishedYear = 2005
        },
        new Book
        {
            Title = "The Pillars of the Earth",
            Author = "Ken Follett",
            Genre = "Historical Fiction",
            Description = "The building of a cathedral in twelfth-century England spans decades of famine, " +
                          "civil war and ecclesiastical politics. Masons, monks and nobles fight over stone, " +
                          "land and ambition.",
            Rating = 4.3,
            PublishedYear = 1989
        },
        new Book
        {
            Title = "Hamnet",
            Author = "Maggie O'Farrell",
            Genre = "Historical Fiction",
            Description = "In plague-era Stratford, a glovemaker's son leaves for London while his wife raises " +
                          "their children alone. The death of their boy reshapes a marriage and echoes into a " +
                          "famous play.",
            Rating = 4.2,
            PublishedYear = 2020
        },
        new Book
        {
            Title = "The Nightingale",
            Author = "Kristin Hannah",
            Genre = "Historical Fiction",
            Description = "Two French sisters take opposite paths under German occupation, one sheltering " +
                          "refugees and one smuggling airmen over the Pyrenees. Their choices carry costs that " +
                          "outlast the liberation.",
            Rating = 4.5,
            PublishedYear = 2015
        },

        // ---------------- Horror ----------------
        new Book
        {
            Title = "Dracula",
            Author = "Bram Stoker",
            Genre = "Horror",
            Description = "Letters and diaries trace a Transylvanian count's move to London and the plague of " +
                          "sleepwalking and blood loss that follows. A doctor, a solicitor and a professor " +
                          "hunt the vampire back to his castle.",
            Rating = 4.2,
            PublishedYear = 1897
        },
        new Book
        {
            Title = "The Shining",
            Author = "Stephen King",
            Genre = "Horror",
            Description = "A recovering alcoholic takes a winter caretaking job at an isolated mountain hotel " +
                          "with his wife and psychic son. Snow seals them in while the building's dead " +
                          "residents work on the father's mind.",
            Rating = 4.4,
            PublishedYear = 1977
        },
        new Book
        {
            Title = "The Haunting of Hill House",
            Author = "Shirley Jackson",
            Genre = "Horror",
            Description = "Four people spend a summer in a mansion with a reputation for madness and " +
                          "disappearances. The house works on the most fragile of them until no one can tell " +
                          "haunting from breakdown.",
            Rating = 4.1,
            PublishedYear = 1959
        },
        new Book
        {
            Title = "Frankenstein",
            Author = "Mary Shelley",
            Genre = "Horror",
            Description = "A student assembles a living creature from dead tissue and abandons it in disgust. " +
                          "The rejected creation pursues its maker across Europe demanding companionship and " +
                          "revenge.",
            Rating = 4.1,
            PublishedYear = 1818
        },
        new Book
        {
            Title = "Mexican Gothic",
            Author = "Silvia Moreno-Garcia",
            Genre = "Horror",
            Description = "A glamorous socialite travels to a decaying silver-mining mansion to rescue her " +
                          "cousin from a English family. Fungus, eugenics and colonial rot fester behind the " +
                          "wallpaper of High Place.",
            Rating = 4.0,
            PublishedYear = 2020
        },
        new Book
        {
            Title = "Bird Box",
            Author = "Josh Malerman",
            Genre = "Horror",
            Description = "Something outside drives anyone who sees it to violent suicide, so survivors live " +
                          "blindfolded behind boarded windows. A mother rows two children downriver without " +
                          "opening her eyes.",
            Rating = 3.9,
            PublishedYear = 2014
        },

        // ---------------- Romance ----------------
        new Book
        {
            Title = "Pride and Prejudice",
            Author = "Jane Austen",
            Genre = "Romance",
            Description = "A sharp-tongued country gentlewoman clashes with a wealthy, reserved landowner over " +
                          "manners and money. Misjudgement on both sides gives way to affection as family " +
                          "scandal threatens their prospects.",
            Rating = 4.6,
            PublishedYear = 1813
        },
        new Book
        {
            Title = "Jane Eyre",
            Author = "Charlotte Bronte",
            Genre = "Romance",
            Description = "An orphaned governess takes a post at a remote hall and falls for her brooding " +
                          "employer. A secret locked on the third floor forces her to choose between love and " +
                          "self-respect.",
            Rating = 4.4,
            PublishedYear = 1847
        },
        new Book
        {
            Title = "Beach Read",
            Author = "Emily Henry",
            Genre = "Romance",
            Description = "Two rival novelists rent neighbouring lake houses and dare each other to swap " +
                          "genres for the summer. Grief, deadlines and old resentments complicate a romance " +
                          "neither planned.",
            Rating = 4.0,
            PublishedYear = 2020
        },
        new Book
        {
            Title = "The Time Traveler's Wife",
            Author = "Audrey Niffenegger",
            Genre = "Romance",
            Description = "A man with a genetic disorder slips involuntarily through time, meeting his future " +
                          "wife when she is a child. Their marriage is built around absences neither of them " +
                          "can schedule.",
            Rating = 4.1,
            PublishedYear = 2003
        },
        new Book
        {
            Title = "Normal People",
            Author = "Sally Rooney",
            Genre = "Romance",
            Description = "Two Irish teenagers with mismatched social standing keep finding and losing each " +
                          "other through school and university. Class, silence and self-sabotage shape a " +
                          "relationship neither can name.",
            Rating = 3.9,
            PublishedYear = 2018
        },
        new Book
        {
            Title = "Outlander",
            Author = "Diana Gabaldon",
            Genre = "Romance",
            Description = "A postwar army nurse touches a standing stone and wakes in eighteenth-century " +
                          "Scotland. Torn between two husbands and two centuries, she is drawn into Highland " +
                          "clan politics before the rising.",
            Rating = 4.3,
            PublishedYear = 1991
        },

        // ---------------- Popular Science ----------------
        new Book
        {
            Title = "A Brief History of Time",
            Author = "Stephen Hawking",
            Genre = "Popular Science",
            Description = "A cosmologist explains black holes, the big bang and the arrow of time without " +
                          "equations. The book traces how relativity and quantum mechanics resist being joined " +
                          "into one theory.",
            Rating = 4.3,
            PublishedYear = 1988
        },
        new Book
        {
            Title = "The Selfish Gene",
            Author = "Richard Dawkins",
            Genre = "Popular Science",
            Description = "Evolution is retold from the gene's point of view, with organisms as vehicles built " +
                          "to carry replicators forward. The argument reframes altruism, kinship and " +
                          "competition in biology.",
            Rating = 4.2,
            PublishedYear = 1976
        },
        new Book
        {
            Title = "Sapiens",
            Author = "Yuval Noah Harari",
            Genre = "Popular Science",
            Description = "A sweeping history of humans from foraging bands to global empires, driven by shared " +
                          "fictions like money, nations and religion. Agriculture, science and capitalism are " +
                          "examined as cognitive revolutions.",
            Rating = 4.4,
            PublishedYear = 2011
        },
        new Book
        {
            Title = "The Immortal Life of Henrietta Lacks",
            Author = "Rebecca Skloot",
            Genre = "Popular Science",
            Description = "Cells taken without consent from a poor tobacco farmer became the immortal line " +
                          "behind decades of medical research. Her family's story raises hard questions about " +
                          "consent, race and profit in science.",
            Rating = 4.3,
            PublishedYear = 2010
        },
        new Book
        {
            Title = "Cosmos",
            Author = "Carl Sagan",
            Genre = "Popular Science",
            Description = "An astronomer tours the universe from the origin of stars to the search for " +
                          "extraterrestrial intelligence. Science is presented as a candle in the dark against " +
                          "superstition and self-destruction.",
            Rating = 4.5,
            PublishedYear = 1980
        },
        new Book
        {
            Title = "Entangled Life",
            Author = "Merlin Sheldrake",
            Genre = "Popular Science",
            Description = "Fungi are shown networking forests, digesting rock and altering animal behaviour. " +
                          "The biology of mycelium challenges tidy definitions of individuality and " +
                          "intelligence.",
            Rating = 4.3,
            PublishedYear = 2020
        },

        // ---------------- Technology ----------------
        new Book
        {
            Title = "Life 3.0: Being Human in the Age of Artificial Intelligence",
            Author = "Max Tegmark",
            Genre = "Technology",
            Description = "A physicist maps possible futures in which artificial intelligence matches and " +
                          "exceeds human capability. Alignment, machine goals and the economics of automation " +
                          "are argued through concrete scenarios.",
            Rating = 4.1,
            PublishedYear = 2017
        },
        new Book
        {
            Title = "Superintelligence",
            Author = "Nick Bostrom",
            Genre = "Technology",
            Description = "A philosopher analyses what happens if machine intelligence surpasses our own and " +
                          "how control might be retained. Recursive self-improvement, instrumental goals and " +
                          "value loading are examined in detail.",
            Rating = 3.9,
            PublishedYear = 2014
        },
        new Book
        {
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt",
            Genre = "Technology",
            Description = "Practical advice on software craftsmanship, from orthogonal design and automation to " +
                          "debugging discipline. Each tip targets the habits that keep code and careers " +
                          "maintainable.",
            Rating = 4.5,
            PublishedYear = 1999
        },
        new Book
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Genre = "Technology",
            Description = "A guide to writing software that other programmers can read, with rules for naming, " +
                          "functions and error handling. Extended refactoring case studies show messy code " +
                          "being cleaned step by step.",
            Rating = 4.2,
            PublishedYear = 2008
        },
        new Book
        {
            Title = "Weapons of Math Destruction",
            Author = "Cathy O'Neil",
            Genre = "Technology",
            Description = "Opaque algorithms score people for credit, policing and hiring while escaping " +
                          "scrutiny. A data scientist shows how these machine learning models scale bias into " +
                          "public life.",
            Rating = 4.1,
            PublishedYear = 2016
        },
        new Book
        {
            Title = "The Alignment Problem",
            Author = "Brian Christian",
            Genre = "Technology",
            Description = "Machine learning systems learn objectives we did not intend, from biased " +
                          "classifiers to reward-hacking agents. The book follows researchers trying to make " +
                          "artificial intelligence reflect human values.",
            Rating = 4.3,
            PublishedYear = 2020
        },
        new Book
        {
            Title = "Code: The Hidden Language of Computer Hardware and Software",
            Author = "Charles Petzold",
            Genre = "Technology",
            Description = "Starting with flashlights and Morse code, the book builds up to logic gates, memory " +
                          "and a working computer. It explains how binary, circuits and instruction sets " +
                          "actually fit together.",
            Rating = 4.6,
            PublishedYear = 1999
        },

        // ---------------- Business & Self-Help ----------------
        new Book
        {
            Title = "Atomic Habits",
            Author = "James Clear",
            Genre = "Business",
            Description = "Small repeated behaviours compound into identity, so the book focuses on cues, " +
                          "friction and environment design. Habit stacking and tracking replace reliance on " +
                          "motivation.",
            Rating = 4.5,
            PublishedYear = 2018
        },
        new Book
        {
            Title = "Thinking, Fast and Slow",
            Author = "Daniel Kahneman",
            Genre = "Business",
            Description = "Two systems of thought, one intuitive and one deliberate, explain a catalogue of " +
                          "cognitive biases. Decades of experiments on judgement under uncertainty are " +
                          "summarised for decision makers.",
            Rating = 4.3,
            PublishedYear = 2011
        },
        new Book
        {
            Title = "The Lean Startup",
            Author = "Eric Ries",
            Genre = "Business",
            Description = "Startups are framed as experiments that should test assumptions with minimum viable " +
                          "products. Build-measure-learn loops and validated learning replace elaborate " +
                          "business plans.",
            Rating = 4.0,
            PublishedYear = 2011
        },
        new Book
        {
            Title = "Deep Work",
            Author = "Cal Newport",
            Genre = "Business",
            Description = "Concentrated, distraction-free effort is treated as a rare and valuable skill in a " +
                          "connected economy. Rituals, scheduling and quitting social media are offered as " +
                          "practical training.",
            Rating = 4.2,
            PublishedYear = 2016
        },
        new Book
        {
            Title = "Good to Great",
            Author = "Jim Collins",
            Genre = "Business",
            Description = "A research team compares companies that made a sustained leap in performance with " +
                          "those that did not. Disciplined people, hedgehog strategy and level five leadership " +
                          "emerge as the differences.",
            Rating = 4.0,
            PublishedYear = 2001
        },
        new Book
        {
            Title = "Never Split the Difference",
            Author = "Chris Voss",
            Genre = "Business",
            Description = "A former hostage negotiator adapts crisis tactics to everyday bargaining. Tactical " +
                          "empathy, calibrated questions and mirroring are drilled through real negotiation " +
                          "transcripts.",
            Rating = 4.4,
            PublishedYear = 2016
        },

        // ---------------- Philosophy ----------------
        new Book
        {
            // Modern editions/translations are used throughout so PublishedYear stays a real,
            // in-range value rather than a fudged date for an ancient text.
            Title = "Meditations: A New Translation",
            Author = "Marcus Aurelius",
            Genre = "Philosophy",
            Description = "Private notes of a Roman emperor on duty, mortality and the discipline of judgement. " +
                          "Stoic practice is applied to anger, grief and the demands of public office.",
            Rating = 4.4,
            PublishedYear = 2002
        },
        new Book
        {
            Title = "Sophie's World",
            Author = "Jostein Gaarder",
            Genre = "Philosophy",
            Description = "A teenager receives anonymous letters posing the basic questions of philosophy and " +
                          "is led through its history. The frame story turns into a puzzle about who is " +
                          "narrating whom.",
            Rating = 4.0,
            PublishedYear = 1991
        },
        new Book
        {
            Title = "Beyond Good and Evil",
            Author = "Friedrich Nietzsche",
            Genre = "Philosophy",
            Description = "A polemic against inherited morality, dogmatic philosophy and the herd instinct. " +
                          "Aphorisms press towards a revaluation of values and the will to power.",
            Rating = 4.0,
            PublishedYear = 1886
        },
        new Book
        {
            Title = "The Myth of Sisyphus",
            Author = "Albert Camus",
            Genre = "Philosophy",
            Description = "If life is absurd, the essay asks, why not end it, and answers by embracing revolt " +
                          "without appeal. Sisyphus pushing his rock becomes the model of a life lived without " +
                          "consolation.",
            Rating = 4.2,
            PublishedYear = 1942
        },
        new Book
        {
            Title = "Man's Search for Meaning",
            Author = "Viktor Frankl",
            Genre = "Philosophy",
            Description = "A psychiatrist recounts surviving concentration camps and the inner choices that " +
                          "sustained prisoners. From that experience he builds a therapy centred on meaning " +
                          "rather than pleasure or power.",
            Rating = 4.6,
            PublishedYear = 1946
        },
        new Book
        {
            Title = "Justice: What's the Right Thing to Do?",
            Author = "Michael Sandel",
            Genre = "Philosophy",
            Description = "Utilitarian, libertarian and Kantian arguments are tested against price gouging, " +
                          "conscription and affirmative action. Moral reasoning is taught through cases rather " +
                          "than abstractions.",
            Rating = 4.3,
            PublishedYear = 2009
        },

        // ---------------- Biography & Memoir ----------------
        new Book
        {
            Title = "Educated",
            Author = "Tara Westover",
            Genre = "Biography",
            Description = "Raised by survivalist parents who kept her out of school, the author teaches herself " +
                          "enough to reach university. Education pulls her away from family loyalty at " +
                          "enormous personal cost.",
            Rating = 4.5,
            PublishedYear = 2018
        },
        new Book
        {
            Title = "The Diary of a Young Girl",
            Author = "Anne Frank",
            Genre = "Biography",
            Description = "A teenager records two years hiding from Nazi occupation in a concealed annex. Her " +
                          "diary balances ordinary adolescence against the constant fear of discovery.",
            Rating = 4.5,
            PublishedYear = 1947
        },
        new Book
        {
            Title = "Steve Jobs",
            Author = "Walter Isaacson",
            Genre = "Biography",
            Description = "Built on dozens of interviews, this biography traces the founder of Apple from " +
                          "garage projects to the iPhone. Product obsession, cruelty and reinvention run " +
                          "through every chapter.",
            Rating = 4.2,
            PublishedYear = 2011
        },
        new Book
        {
            Title = "Becoming",
            Author = "Michelle Obama",
            Genre = "Biography",
            Description = "A memoir moving from the South Side of Chicago through law, marriage and the White " +
                          "House. Public scrutiny and private identity are weighed against each other " +
                          "throughout.",
            Rating = 4.4,
            PublishedYear = 2018
        },
        new Book
        {
            Title = "When Breath Becomes Air",
            Author = "Paul Kalanithi",
            Genre = "Biography",
            Description = "A neurosurgeon diagnosed with terminal lung cancer writes about practising medicine " +
                          "and then receiving it. The memoir asks what makes a life worth living when time is " +
                          "short.",
            Rating = 4.6,
            PublishedYear = 2016
        },
        new Book
        {
            Title = "The Wright Brothers",
            Author = "David McCullough",
            Genre = "Biography",
            Description = "Two bicycle mechanics from Ohio solve controlled flight through relentless " +
                          "experiment and wind tunnel testing. Letters and diaries show the discipline behind " +
                          "the first powered aeroplane.",
            Rating = 4.1,
            PublishedYear = 2015
        }
    ];

    private static List<User> BuildUsers() =>
    [
        new User
        {
            // Heavy ratings, sci-fi and AI leaning.
            Name = "Aisha Khan",
            FavouriteGenres = "Science Fiction, Technology",
            FavouriteAuthors = "Isaac Asimov, William Gibson"
        },
        new User
        {
            // Crime and horror reader.
            Name = "Ben Carter",
            FavouriteGenres = "Mystery, Horror",
            FavouriteAuthors = "Agatha Christie, Stephen King"
        },
        new User
        {
            // Romance and historical fiction.
            Name = "Chloe Martin",
            FavouriteGenres = "Romance, Historical Fiction",
            FavouriteAuthors = "Jane Austen, Kristin Hannah"
        },
        new User
        {
            // Cold start: stated preferences but no ratings yet.
            Name = "Daniel Okafor",
            FavouriteGenres = "Popular Science, Philosophy",
            FavouriteAuthors = "Carl Sagan"
        },
        new User
        {
            // Cold start: brand new account, nothing known at all.
            Name = "Emma Reid",
            FavouriteGenres = "",
            FavouriteAuthors = ""
        }
    ];

    private static List<Rating> BuildRatings(
        IReadOnlyDictionary<string, int> users,
        IReadOnlyDictionary<string, int> books)
    {
        (string User, string Title, int Stars)[] seed =
        [
            // Aisha: loves science fiction and AI, lukewarm on everything else.
            ("Aisha Khan", "Foundation", 5),
            ("Aisha Khan", "I, Robot", 5),
            ("Aisha Khan", "Neuromancer", 5),
            ("Aisha Khan", "The Alignment Problem", 4),
            ("Aisha Khan", "Snow Crash", 4),
            ("Aisha Khan", "Pride and Prejudice", 2),
            ("Aisha Khan", "Beach Read", 1),

            // Ben: crime and horror.
            ("Ben Carter", "And Then There Were None", 5),
            ("Ben Carter", "The Shining", 5),
            ("Ben Carter", "The Big Sleep", 4),
            ("Ben Carter", "The Haunting of Hill House", 4),
            ("Ben Carter", "Gone Girl", 4),
            ("Ben Carter", "Sapiens", 3),
            ("Ben Carter", "Atomic Habits", 2),

            // Chloe: romance and historical fiction.
            ("Chloe Martin", "Pride and Prejudice", 5),
            ("Chloe Martin", "The Nightingale", 5),
            ("Chloe Martin", "Jane Eyre", 4),
            ("Chloe Martin", "The Book Thief", 4),
            ("Chloe Martin", "Outlander", 4),
            ("Chloe Martin", "Neuromancer", 2),
            ("Chloe Martin", "Clean Code", 1)

            // Daniel and Emma deliberately have no ratings — they exercise the cold-start paths.
        ];

        return seed
            .Where(s => users.ContainsKey(s.User) && books.ContainsKey(s.Title))
            .Select((s, index) => new Rating
            {
                UserId = users[s.User],
                BookId = books[s.Title],
                Stars = s.Stars,
                RatedDate = DateTime.UtcNow.AddDays(-index)
            })
            .ToList();
    }

    private static List<ReadingHistory> BuildReadingHistory(
        IReadOnlyDictionary<string, int> users,
        IReadOnlyDictionary<string, int> books)
    {
        (string User, string Title, ReadingStatus Status)[] seed =
        [
            ("Aisha Khan", "Foundation", ReadingStatus.Read),
            ("Aisha Khan", "I, Robot", ReadingStatus.Read),
            ("Aisha Khan", "Neuromancer", ReadingStatus.Read),
            ("Aisha Khan", "Dune", ReadingStatus.Reading),
            ("Aisha Khan", "Superintelligence", ReadingStatus.WantToRead),

            ("Ben Carter", "And Then There Were None", ReadingStatus.Read),
            ("Ben Carter", "The Shining", ReadingStatus.Read),
            ("Ben Carter", "Dracula", ReadingStatus.Reading),
            ("Ben Carter", "Mexican Gothic", ReadingStatus.WantToRead),

            ("Chloe Martin", "Pride and Prejudice", ReadingStatus.Read),
            ("Chloe Martin", "The Nightingale", ReadingStatus.Read),
            ("Chloe Martin", "Hamnet", ReadingStatus.Reading),
            ("Chloe Martin", "Wolf Hall", ReadingStatus.WantToRead),

            ("Daniel Okafor", "Cosmos", ReadingStatus.WantToRead)
        ];

        return seed
            .Where(s => users.ContainsKey(s.User) && books.ContainsKey(s.Title))
            .Select(s => new ReadingHistory
            {
                UserId = users[s.User],
                BookId = books[s.Title],
                Status = s.Status
            })
            .ToList();
    }
}
