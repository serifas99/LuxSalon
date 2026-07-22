import 'package:ecommerce_desktop/layouts/master_screen.dart';
import 'package:ecommerce_desktop/models/frizer.dart';
import 'package:ecommerce_desktop/models/search_result.dart';
import 'package:ecommerce_desktop/models/user.dart';
import 'package:ecommerce_desktop/models/usluga.dart';
import 'package:ecommerce_desktop/providers/frizer_provider.dart';
import 'package:ecommerce_desktop/providers/user_provider.dart';
import 'package:ecommerce_desktop/providers/usluga_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_form_builder/flutter_form_builder.dart';
import 'package:provider/provider.dart';

class FrizerDetailsScreen extends StatefulWidget {
  final Frizer? frizer;

  const FrizerDetailsScreen({super.key, this.frizer});

  @override
  State<FrizerDetailsScreen> createState() => _FrizerDetailsScreenState();
}

class _FrizerDetailsScreenState extends State<FrizerDetailsScreen> {
  final _formKey = GlobalKey<FormBuilderState>();
  Map<String, dynamic> _initialValue = {};

  late FrizerProvider _provider;
  late UslugaProvider _uslugaProvider;
  late UserProvider _userProvider;
  SearchResult<Usluga>? _uslugeResult;
  SearchResult<User>? _korisniciResult;

  List<int> _odabraneUsluge = [];
  bool isLoading = true;

  bool get _isEditing => widget.frizer != null;

  @override
  void initState() {
    super.initState();

    _odabraneUsluge = widget.frizer?.uslugaIds ?? [];

    _initialValue = {
      'userId': widget.frizer?.userId,
      'biografija': widget.frizer?.biografija ?? '',
      'specijalizacija': widget.frizer?.specijalizacija ?? '',
      'isActive': widget.frizer?.isActive ?? true,
    };

    _provider = context.read<FrizerProvider>();
    _uslugaProvider = context.read<UslugaProvider>();
    _userProvider = context.read<UserProvider>();
    initForm();
  }

  Future initForm() async {
    var usluge = await _uslugaProvider.get(filter: {"pageSize": 1000});
    SearchResult<User>? korisnici;
    if (!_isEditing) {
      korisnici = await _userProvider.get(filter: {"pageSize": 1000});
    }

    if (!mounted) return;

    setState(() {
      isLoading = false;
      _uslugeResult = usluge;
      _korisniciResult = korisnici;
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return MasterScreen(
      title: widget.frizer != null ? 'Uredi frizera' : 'Novi frizer',
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
            Icons.person_outline,
            color: theme.colorScheme.onPrimaryContainer,
          ),
        ),
        const SizedBox(width: 16.0),
        Column(
          children: [
            Text(
              widget.frizer != null ? widget.frizer!.imePrezime! : 'Novi frizer',
              style: theme.textTheme.headlineSmall,
            ),
            Text(
              widget.frizer != null
                  ? 'Uredite podatke frizera'
                  : 'Popunite formu za novog frizera',
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
          if (_isEditing) ...[
            // Ime, prezime i email dolaze sa korisničkog naloga i ne mogu se
            // mijenjati ovdje (to je posao ekrana Klijenti / User Management).
            Align(
              alignment: Alignment.centerLeft,
              child: Text("${widget.frizer!.imePrezime ?? ''}  •  ${widget.frizer!.email ?? ''}",
                  style: theme.textTheme.titleSmall),
            ),
            const SizedBox(height: 16.0),
          ] else ...[
            FormBuilderDropdown<int>(
              name: 'userId',
              decoration: const InputDecoration(
                label: Text("Korisnički nalog"),
                helperText: "Izaberi postojeći korisnički nalog koji postaje frizer",
              ),
              items: [
                ...?_korisniciResult?.items?.map(
                  (u) => DropdownMenuItem(
                    value: u.id,
                    child: Text(
                        "${u.firstName ?? ''} ${u.lastName ?? ''} (${u.username ?? ''})"),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16.0),
          ],
          FormBuilderTextField(
            name: 'specijalizacija',
            decoration: const InputDecoration(label: Text("Specijalizacija")),
          ),
          const SizedBox(height: 16.0),
          FormBuilderTextField(
            name: 'biografija',
            decoration: const InputDecoration(label: Text("Biografija")),
            maxLines: 3,
          ),
          const SizedBox(height: 16.0),
          FormBuilderCheckbox(
            name: 'isActive',
            title: const Text("Aktivan"),
          ),
          const SizedBox(height: 16.0),
          Align(
            alignment: Alignment.centerLeft,
            child: Text("Usluge koje pruža", style: theme.textTheme.titleSmall),
          ),
          Wrap(
            spacing: 8,
            children: (_uslugeResult?.items ?? [])
                .map(
                  (u) => FilterChip(
                    label: Text(u.naziv ?? ''),
                    selected: _odabraneUsluge.contains(u.id),
                    onSelected: (selected) {
                      setState(() {
                        if (selected) {
                          _odabraneUsluge.add(u.id!);
                        } else {
                          _odabraneUsluge.remove(u.id);
                        }
                      });
                    },
                  ),
                )
                .toList(),
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
      formData['uslugaIds'] = _odabraneUsluge;

      if (!_isEditing && formData['userId'] == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Izaberi korisnički nalog za frizera")),
        );
        return;
      }

      if (_isEditing) {
        // FrizerUpdateRequest ne prima userId - ne šaljemo ga.
        formData.remove('userId');
      }

      try {
        if (widget.frizer != null) {
          await _provider.update(widget.frizer!.id!, formData);
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
