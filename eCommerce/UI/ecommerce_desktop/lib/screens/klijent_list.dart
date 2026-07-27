import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/klijent_pregled.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/providers/user_provider.dart';
import 'package:ecommerce_desktop/screens/user_details_screen.dart';
import 'package:ecommerce_desktop/utils/utils_widgets.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

/// Ekran "Klijenti" - tacno po prijavi teme (5.5): tabela sa ime i prezime, email, broj
/// zakazanih termina i podaci o posljednjem terminu, dugme "Uredi" u koloni Akcija,
/// paginacija na dnu ekrana.
class KlijentList extends StatefulWidget {
  const KlijentList({super.key});

  @override
  State<KlijentList> createState() => _KlijentListState();
}

class _KlijentListState extends State<KlijentList> {
  late UserProvider _userProvider;
  final TextEditingController _nameController = TextEditingController();

  SearchResult<KlijentPregled>? _result;
  bool _isLoading = true;
  int _page = 1;
  static const int _pageSize = 10;

  @override
  void initState() {
    super.initState();
    _userProvider = context.read<UserProvider>();
    _ucitaj();
  }

  Future _ucitaj() async {
    setState(() => _isLoading = true);
    try {
      final rezultat = await _userProvider.klijenti(filter: {
        "page": _page,
        "pageSize": _pageSize,
        "includeTotalCount": true,
        if (_nameController.text.isNotEmpty) "name": _nameController.text,
      });
      setState(() {
        _result = rezultat;
        _isLoading = false;
      });
    } on Exception catch (e) {
      setState(() => _isLoading = false);
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  void _pretrazi() {
    _page = 1;
    _ucitaj();
  }

  Future _uredi(KlijentPregled klijent) async {
    try {
      final user = await _userProvider.getById(klijent.id!);
      if (!mounted) return;
      final refresh = await Navigator.of(context).push(
        MaterialPageRoute(builder: (context) => UserDetailsScreen(user: user)),
      );
      if (refresh == "reload") _ucitaj();
    } on Exception catch (e) {
      if (mounted) alertBox(context, "Greška", e.toString());
    }
  }

  String _formatDatum(DateTime? d) {
    if (d == null) return '-';
    return "${d.day.toString().padLeft(2, '0')}.${d.month.toString().padLeft(2, '0')}.${d.year}.";
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return MasterScreen(
      title: "Klijenti",
      child: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.people, color: theme.colorScheme.primary, size: 28),
                const SizedBox(width: 12),
                Text('Klijenti',
                    style: theme.textTheme.headlineSmall
                        ?.copyWith(fontWeight: FontWeight.bold)),
              ],
            ),
            const SizedBox(height: 20),
            Card(
              elevation: 2,
              shape:
                  RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                child: Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _nameController,
                        decoration: InputDecoration(
                          labelText: "Pretraga po imenu/prezimenu",
                          prefixIcon: const Icon(Icons.search),
                          border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8)),
                          isDense: true,
                        ),
                        onSubmitted: (_) => _pretrazi(),
                      ),
                    ),
                    const SizedBox(width: 12),
                    ElevatedButton.icon(
                      onPressed: _pretrazi,
                      icon: const Icon(Icons.search),
                      label: const Text("Pretraži"),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            if (_isLoading)
              const Expanded(child: Center(child: CircularProgressIndicator()))
            else
              _buildTable(theme),
          ],
        ),
      ),
    );
  }

  Widget _buildTable(ThemeData theme) {
    final klijenti = _result?.items ?? [];
    final totalCount = _result?.totalCount ?? 0;
    final ukupnoStranica =
        totalCount == 0 ? 1 : ((totalCount - 1) ~/ _pageSize) + 1;

    if (klijenti.isEmpty) {
      return Expanded(
        child: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.people_outline,
                  size: 64, color: theme.colorScheme.outline),
              const SizedBox(height: 12),
              Text('Nema klijenata',
                  style: theme.textTheme.titleMedium
                      ?.copyWith(color: theme.colorScheme.outline)),
            ],
          ),
        ),
      );
    }

    return Expanded(
      child: Column(
        children: [
          Expanded(
            child: Card(
              elevation: 2,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12)),
              clipBehavior: Clip.antiAlias,
              child: SingleChildScrollView(
                child: DataTable(
                  headingRowColor: WidgetStateProperty.all(theme
                      .colorScheme.primaryContainer
                      .withValues(alpha: 0.5)),
                  headingTextStyle: theme.textTheme.labelLarge
                      ?.copyWith(fontWeight: FontWeight.bold),
                  columnSpacing: 24,
                  columns: const [
                    DataColumn(label: Text("Ime i prezime")),
                    DataColumn(label: Text("Email")),
                    DataColumn(label: Text("Broj zakazanih termina")),
                    DataColumn(label: Text("Posljednji termin")),
                    DataColumn(label: Text("Akcija")),
                  ],
                  rows: klijenti.map((k) {
                    return DataRow(cells: [
                      DataCell(Text(k.imePrezime ?? '-')),
                      DataCell(Text(k.email ?? '-')),
                      DataCell(Text("${k.brojZakazanihTermina ?? 0}")),
                      DataCell(Text(_formatDatum(k.datumPosljednjegTermina))),
                      DataCell(
                        IconButton(
                          tooltip: "Uredi",
                          icon: Icon(Icons.edit_outlined,
                              color: theme.colorScheme.primary),
                          onPressed: () => _uredi(k),
                        ),
                      ),
                    ]);
                  }).toList(),
                ),
              ),
            ),
          ),
          const SizedBox(height: 12),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              IconButton(
                icon: const Icon(Icons.chevron_left),
                onPressed: _page > 1
                    ? () {
                        setState(() => _page--);
                        _ucitaj();
                      }
                    : null,
              ),
              Text("Stranica $_page od $ukupnoStranica"),
              IconButton(
                icon: const Icon(Icons.chevron_right),
                onPressed: _page < ukupnoStranica
                    ? () {
                        setState(() => _page++);
                        _ucitaj();
                      }
                    : null,
              ),
            ],
          ),
        ],
      ),
    );
  }
}
