import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/models/usluga.dart';
import 'package:ecommerce_desktop/models/usluga_kategorija.dart';
import 'package:ecommerce_desktop/providers/usluga_kategorija_provider.dart';
import 'package:ecommerce_desktop/providers/usluga_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:provider/provider.dart';

class UslugaDetailsScreen extends StatefulWidget {
  final Usluga? usluga;

  const UslugaDetailsScreen({super.key, this.usluga});

  @override
  State<UslugaDetailsScreen> createState() => _UslugaDetailsScreenState();
}

class _UslugaDetailsScreenState extends State<UslugaDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  Map<String, dynamic> _initialValue = {};

  late UslugaProvider _provider;
  late UslugaKategorijaProvider _kategorijaProvider;
  SearchResult<UslugaKategorija>? _kategorijeResult;

  bool isLoading = true;

  @override
  void initState() {
    super.initState();

    _initialValue = {
      'naziv': widget.usluga?.naziv ?? '',
      'opis': widget.usluga?.opis ?? '',
      'cijena': widget.usluga?.cijena?.toString() ?? '',
      'trajanjeMinuta': widget.usluga?.trajanjeMinuta?.toString() ?? '',
      'uslugaKategorijaId': widget.usluga?.uslugaKategorijaId,
      'tagovi': widget.usluga?.tagovi ?? '',
      'isActive': widget.usluga?.isActive ?? true,
    };

    _provider = context.read<UslugaProvider>();
    _kategorijaProvider = context.read<UslugaKategorijaProvider>();
    initForm();
  }

  Future initForm() async {
    var kategorije = await _kategorijaProvider.get(filter: {});

    if (!mounted) return;

    setState(() {
      isLoading = false;
      _kategorijeResult = kategorije;
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return MasterScreen(
      title: widget.usluga != null ? 'Uredi uslugu' : 'Nova usluga',
      child: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(32.0),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 700),
            child: Column(
              children: [
                _buildHeader(theme),
                const SizedBox(height: 24.0),
                Card(
                  elevation: 10,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12.0),
                    side: BorderSide(
                      color: theme.colorScheme.primaryContainer,
                      width: 2,
                    ),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: isLoading
                        ? const Center(child: CircularProgressIndicator())
                        : _buildForm(theme),
                  ),
                ),
                SizedBox(height: 24.0),
                _buildActions(theme),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeader(ThemeData theme) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(10.0),
          decoration: BoxDecoration(
            color: theme.colorScheme.primaryContainer,
            borderRadius: BorderRadius.circular(8.0),
          ),
          child: Icon(
            Icons.content_cut,
            color: theme.colorScheme.onPrimaryContainer,
          ),
        ),
        const SizedBox(width: 16.0),
        Column(
          children: [
            Text(
              widget.usluga != null ? widget.usluga!.naziv! : 'Nova usluga',
              style: theme.textTheme.headlineSmall,
            ),
            Text(
              widget.usluga != null
                  ? 'Uredite podatke usluge'
                  : 'Popunite formu za novu uslugu',
              style: theme.textTheme.bodyMedium,
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildForm(ThemeData theme) {
    return FormBuilder(
      key: _formKey,
      initialValue: _initialValue,
      child: Column(
        children: [
          Row(
            children: [
              Expanded(
                child: FormBuilderTextField(
                  name: 'naziv',
                  decoration: const InputDecoration(label: Text("Naziv")),
                ),
              ),
              const SizedBox(width: 16.0),
              Expanded(
                child: FormBuilderDropdown(
                  name: 'uslugaKategorijaId',
                  decoration:
                      const InputDecoration(label: Text("Kategorija")),
                  items: [
                    ...?_kategorijeResult?.items?.map(
                      (k) => DropdownMenuItem(
                        value: k.id,
                        child: Text(k.naziv ?? ''),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16.0),
          FormBuilderTextField(
            name: 'opis',
            decoration: const InputDecoration(label: Text("Opis")),
            maxLines: 3,
          ),
          const SizedBox(height: 16.0),
          Row(
            children: [
              Expanded(
                child: FormBuilderTextField(
                  name: 'cijena',
                  decoration: const InputDecoration(label: Text("Cijena (KM)")),
                  keyboardType: TextInputType.number,
                ),
              ),
              const SizedBox(width: 16.0),
              Expanded(
                child: FormBuilderTextField(
                  name: 'trajanjeMinuta',
                  decoration:
                      const InputDecoration(label: Text("Trajanje (min)")),
                  keyboardType: TextInputType.number,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16.0),
          FormBuilderTextField(
            name: 'tagovi',
            decoration: const InputDecoration(
              label: Text("Tagovi (odvojeni zarezom)"),
              helperText: "npr. kosa,farbanje,zene",
            ),
          ),
          const SizedBox(height: 16.0),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text("Aktivna"),
          ),
        ],
      ),
    );
  }

  Widget _buildActions(ThemeData theme) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.end,
      children: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text("Otkaži"),
        ),
        const SizedBox(width: 16.0),
        ElevatedButton(
          onPressed: _save,
          child: const Text("Sačuvaj"),
        ),
      ],
    );
  }

  Future _save() async {
    if (_formKey.currentState?.saveAndValidate() ?? false) {
      var formData = Map<String, dynamic>.from(_formKey.currentState!.value);

      formData['cijena'] = double.tryParse(formData['cijena'].toString()) ?? 0;
      formData['trajanjeMinuta'] =
          int.tryParse(formData['trajanjeMinuta'].toString()) ?? 0;

      try {
        if (widget.usluga != null) {
          await _provider.update(widget.usluga!.id!, formData);
        } else {
          await _provider.insert(formData);
        }

        if (!mounted) return;
        Navigator.of(context).pop("reload");
      } catch (e) {
        if (!mounted) return;
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Greška prilikom čuvanja: $e")),
        );
      }
    }
  }
}
